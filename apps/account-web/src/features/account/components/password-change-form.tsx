"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FieldSet,
  Input,
} from "@netmetric/ui";

import { initialMutationState, type MutationState } from "../actions/mutation-state";
import { AccountSection } from "./account-section";
import { SecurityActionResult } from "./security-action-result";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type PasswordChangeFormProps = {
  action: (state: MutationState, formData: FormData) => Promise<MutationState>;
};

function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <Button size="xs" type="submit" disabled={pending}>
      {pending
        ? tAccountClient("account.security.password.changing")
        : tAccountClient("account.security.password.changeSubmit")}
    </Button>
  );
}

export function PasswordChangeForm({ action }: PasswordChangeFormProps) {
  const [state, formAction] = useActionState(action, initialMutationState);

  return (
    <AccountSection
      title={tAccountClient("account.security.password.changeTitle")}
      description={tAccountClient("account.security.password.changeDescription")}
      contentClassName="space-y-4"
    >
      <SecurityActionResult
        state={state}
        successTitle={tAccountClient("account.security.password.changed")}
        errorTitle={tAccountClient("account.security.password.updateFailed")}
      />
      <form action={formAction} className="space-y-4" noValidate>
        <FieldSet className="grid gap-4">
          <PasswordField
            id="currentPassword"
            name="currentPassword"
            label={tAccountClient("account.security.password.current")}
            error={state.fieldErrors?.currentPassword?.[0]}
          />
          <PasswordField
            id="newPassword"
            name="newPassword"
            label={tAccountClient("account.security.password.new")}
            error={state.fieldErrors?.newPassword?.[0]}
          />
          <PasswordField
            id="confirmNewPassword"
            name="confirmNewPassword"
            label={tAccountClient("account.security.password.confirmNew")}
            error={state.fieldErrors?.confirmNewPassword?.[0]}
          />
        </FieldSet>
        <div className="flex flex-wrap items-center gap-2">
          <SubmitButton />
          <Button size="xs" type="reset" variant="outline">
            {tAccountClient("account.common.reset")}
          </Button>
        </div>
      </form>
    </AccountSection>
  );
}

function PasswordField({
  id,
  name,
  label,
  error,
}: {
  id: string;
  name: string;
  label: string;
  error: string | undefined;
}) {
  const describedBy = error ? `${id}-error` : undefined;
  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <FieldContent>
        <Input
          id={id}
          name={name}
          type="password"
          autoComplete={name === "currentPassword" ? "current-password" : "new-password"}
          aria-invalid={Boolean(error)}
          aria-describedby={describedBy}
        />
        <FieldError id={`${id}-error`}>{error}</FieldError>
      </FieldContent>
    </Field>
  );
}
