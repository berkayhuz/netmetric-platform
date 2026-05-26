"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FieldSet,
  Input,
  Textarea,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { useForm, useWatch } from "react-hook-form";

import { CrmFormErrorSummary } from "@/components/forms/crm-form-error-summary";
import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import { booleanOptions } from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { scheduleWorkMeetingAction } from "../actions/work-management-create-actions";
import { meetingFormSchema, type MeetingFormInput } from "./task-form-schema";

const defaults: MeetingFormInput = {
  title: "",
  startsAtUtc: "",
  endsAtUtc: "",
  organizerEmail: "",
  attendeeSummary: "",
  requiresExternalSync: false,
};

export function MeetingForm() {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const form = useForm<MeetingFormInput>({
    resolver: zodResolver(meetingFormSchema),
    defaultValues: defaults,
  });
  const requiresExternalSync = useWatch({
    control: form.control,
    name: "requiresExternalSync",
  });
  const syncDisplayOptions = booleanOptions.map((option) => ({
    value: option.value,
    label: tCrmClient(`crm.common.boolean.${option.value}`),
  }));

  const onSubmit = (values: MeetingFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response = await scheduleWorkMeetingAction(values);
      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof MeetingFormInput, { message: first });
        }
      }
      if (response.status === "success" && response.redirectTo) {
        router.push(response.redirectTo);
        router.refresh();
      }
    });
  };

  return (
    <form className="space-y-6" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormErrorSummary
        {...(result.status === "error" && result.message ? { message: result.message } : {})}
        {...(result.fieldErrors ? { errors: result.fieldErrors } : {})}
      />
      <CrmMutationResult state={result} />

      <FieldSet className="grid gap-4 sm:grid-cols-2">
        <Field className="sm:col-span-2">
          <FieldLabel htmlFor="meeting-title">{tCrmClient("crm.meetings.fields.title")}</FieldLabel>
          <FieldContent>
            <Input id="meeting-title" {...form.register("title")} />
            <FieldError>{form.formState.errors.title?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="meeting-startsAtUtc">
            {tCrmClient("crm.meetings.fields.startsAtUtc")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="meeting-startsAtUtc"
              type="datetime-local"
              {...form.register("startsAtUtc")}
            />
            <FieldError>{form.formState.errors.startsAtUtc?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="meeting-endsAtUtc">
            {tCrmClient("crm.meetings.fields.endsAtUtc")}
          </FieldLabel>
          <FieldContent>
            <Input id="meeting-endsAtUtc" type="datetime-local" {...form.register("endsAtUtc")} />
            <FieldError>{form.formState.errors.endsAtUtc?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="meeting-organizerEmail">
            {tCrmClient("crm.meetings.fields.organizerEmail")}
          </FieldLabel>
          <FieldContent>
            <Input id="meeting-organizerEmail" type="email" {...form.register("organizerEmail")} />
            <FieldError>{form.formState.errors.organizerEmail?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field className="sm:col-span-2">
          <FieldLabel htmlFor="meeting-attendeeSummary">
            {tCrmClient("crm.meetings.fields.attendeeSummary")}
          </FieldLabel>
          <FieldContent>
            <Textarea id="meeting-attendeeSummary" rows={4} {...form.register("attendeeSummary")} />
            <FieldError>{form.formState.errors.attendeeSummary?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="meeting-requiresExternalSync">
            {tCrmClient("crm.meetings.fields.requiresExternalSync")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(requiresExternalSync)}
              onValueChange={(value) =>
                form.setValue("requiresExternalSync", value === "true", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="meeting-requiresExternalSync">
                <SelectValue>
                  {getSelectDisplayLabel(String(requiresExternalSync), syncDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {booleanOptions.map((o) => (
                  <SelectItem key={`meeting-sync-${o.value}`} value={o.value}>
                    {tCrmClient(`crm.common.boolean.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.requiresExternalSync?.message}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
        <Button type="button" variant="outline" onClick={() => router.back()}>
          {tCrmClient("crm.forms.actions.cancel")}
        </Button>
        <Button type="submit" disabled={isPending} aria-busy={isPending}>
          {isPending
            ? tCrmClient("crm.meetings.actions.scheduling")
            : tCrmClient("crm.meetings.actions.schedule")}
        </Button>
      </div>
    </form>
  );
}
