"use client";

import { useActionState, useEffect, useRef, useState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Field, FieldContent, FieldError, FieldLabel, FieldSet } from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  toast,
} from "@netmetric/ui/client";

import type {
  AccountOptionsResponse,
  OrganizationMembershipSummaryResponse,
  UserPreferenceResponse,
} from "@/lib/account-api";
import { getPostLoginDestinationOptions } from "@/lib/post-login-destination";

import { initialMutationState, type MutationState } from "../actions/mutation-state";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountPagePanel } from "./account-page-panel";
import { FaviconManagementPanel } from "./favicon-management-panel";

type PreferencesEditFormProps = {
  preferences: UserPreferenceResponse;
  options: AccountOptionsResponse;
  organizations: OrganizationMembershipSummaryResponse[];
  action: (state: MutationState, formData: FormData) => Promise<MutationState>;
};

function SubmitButton() {
  const { pending } = useFormStatus();

  return (
    <Button size="xs" type="submit" disabled={pending}>
      {pending
        ? tAccountClient("account.common.saving")
        : tAccountClient("account.preferences.save")}
    </Button>
  );
}

export function PreferencesEditForm({
  preferences,
  options,
  organizations,
  action,
}: PreferencesEditFormProps) {
  const [state, formAction] = useActionState(action, initialMutationState);
  const destinationOptions = getPostLoginDestinationOptions();
  const noDefaultOrganization = "__none__";
  const isConflict = state.status === "error" && state.code === "conflict";
  const [defaultOrganizationId, setDefaultOrganizationId] = useState(
    preferences.defaultOrganizationId ?? noDefaultOrganization,
  );
  const [postLoginDestination, setPostLoginDestination] = useState(
    preferences.postLoginDestination,
  );
  const lastPreferencesToastKeyRef = useRef<string | null>(null);

  useEffect(() => {
    if (state.status !== "success" && state.status !== "error") {
      return;
    }

    const toastMessage = state.message ?? "";
    const nextToastKey = `${state.status}:${state.code ?? ""}:${toastMessage}`;
    if (nextToastKey === lastPreferencesToastKeyRef.current) {
      return;
    }

    lastPreferencesToastKeyRef.current = nextToastKey;
    if (state.status === "success") {
      toast.success(tAccountClient("account.preferences.updatedTitle"), {
        description: toastMessage || undefined,
      });
      return;
    }

    toast.error(tAccountClient("account.common.updateFailed"), {
      description: toastMessage || undefined,
      ...(isConflict
        ? {
            action: {
              label: tAccountClient("account.preferences.reloadLatest"),
              onClick: () => {
                window.location.reload();
              },
            },
          }
        : {}),
    });
  }, [isConflict, state.code, state.message, state.status]);

  return (
    <AccountPagePanel
      title={tAccountClient("account.preferences.title")}
      description={tAccountClient("account.preferences.description")}
    >
      <div className="mr-auto w-full min-w-0 space-y-6 lg:w-96">
        <FaviconManagementPanel preferences={preferences} />
        <form action={formAction} className="space-y-4" noValidate>
          <input type="hidden" name="version" value={preferences.version} />
          <FieldSet>
            <SelectField
              id="language"
              name="language"
              label={tAccountClient("account.profile.fields.language")}
              defaultValue={preferences.language}
              error={state.fieldErrors?.language?.[0]}
              options={options.languages}
            />
            <SelectField
              id="timeZone"
              name="timeZone"
              label={tAccountClient("account.profile.fields.timeZone")}
              defaultValue={preferences.timeZone}
              error={state.fieldErrors?.timeZone?.[0]}
              options={options.timeZones}
            />
            <SelectField
              id="theme"
              name="theme"
              label={tAccountClient("account.preferences.theme")}
              defaultValue={preferences.theme}
              error={state.fieldErrors?.theme?.[0]}
              options={options.themes}
            />
            <SelectField
              id="dateFormat"
              name="dateFormat"
              label={tAccountClient("account.preferences.dateFormat")}
              defaultValue={preferences.dateFormat}
              error={state.fieldErrors?.dateFormat?.[0]}
              options={options.dateFormats}
            />
            <Field>
              <FieldLabel htmlFor="postLoginDestination">
                {tAccountClient("account.preferences.postLoginDestination")}
              </FieldLabel>
              <FieldContent>
                <input
                  type="hidden"
                  id="postLoginDestination"
                  name="postLoginDestination"
                  value={postLoginDestination}
                />
                <Select
                  value={postLoginDestination}
                  onValueChange={(nextValue) => setPostLoginDestination(nextValue ?? "Account")}
                >
                  <SelectTrigger
                    size="sm"
                    aria-invalid={Boolean(state.fieldErrors?.postLoginDestination?.[0])}
                  >
                    <SelectValue
                      placeholder={tAccountClient("account.preferences.postLoginDestination")}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {destinationOptions.map((option) => (
                      <SelectItem key={option.key} value={option.key}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError id="postLoginDestination-error">
                  {state.fieldErrors?.postLoginDestination?.[0]}
                </FieldError>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="defaultOrganizationId">
                {tAccountClient("account.preferences.defaultOrganization")}
              </FieldLabel>
              <FieldContent>
                <input
                  type="hidden"
                  id="defaultOrganizationId"
                  name="defaultOrganizationId"
                  value={
                    defaultOrganizationId === noDefaultOrganization ? "" : defaultOrganizationId
                  }
                />
                <Select
                  value={defaultOrganizationId}
                  onValueChange={(nextValue) =>
                    setDefaultOrganizationId(nextValue ?? noDefaultOrganization)
                  }
                >
                  <SelectTrigger size="sm">
                    <SelectValue
                      placeholder={tAccountClient("account.preferences.noDefaultOrganization")}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={noDefaultOrganization}>
                      {tAccountClient("account.preferences.noDefaultOrganization")}
                    </SelectItem>
                    {organizations.map((org) => (
                      <SelectItem key={org.organizationId} value={org.organizationId}>
                        {org.organizationName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError id="defaultOrganizationId-error">
                  {state.fieldErrors?.defaultOrganizationId?.[0]}
                </FieldError>
              </FieldContent>
            </Field>
          </FieldSet>

          <div className="flex flex-wrap items-center gap-2">
            <SubmitButton />
            <Button size="xs" type="reset" variant="outline">
              {tAccountClient("account.common.reset")}
            </Button>
          </div>
        </form>
      </div>
    </AccountPagePanel>
  );
}

function SelectField({
  id,
  name,
  label,
  defaultValue,
  error,
  options,
}: {
  id: string;
  name: string;
  label: string;
  defaultValue: string;
  error: string | undefined;
  options: { value: string; label: string }[];
}) {
  const [value, setValue] = useState(defaultValue);

  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <FieldContent>
        <input type="hidden" id={id} name={name} value={value} />
        <Select value={value} onValueChange={(nextValue) => setValue(nextValue ?? defaultValue)}>
          <SelectTrigger size="sm" aria-invalid={Boolean(error)}>
            <SelectValue
              placeholder={tAccountClient("account.common.selectPlaceholder", { label })}
            />
          </SelectTrigger>
          <SelectContent>
            {options.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <FieldError id={`${id}-error`}>{error}</FieldError>
      </FieldContent>
    </Field>
  );
}
