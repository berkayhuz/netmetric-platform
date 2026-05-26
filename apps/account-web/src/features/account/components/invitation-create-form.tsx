"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Field, FieldContent, FieldError, FieldLabel, Input } from "@netmetric/ui";
import { Send } from "lucide-react";

import { createInvitationAction } from "../actions/invitation-actions";
import { initialMutationState } from "../actions/mutation-state";
import { InvitationActionResult } from "./invitation-action-result";
import { tAccountClient } from "@/lib/i18n/account-i18n";

function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <Button size="xs" type="submit" disabled={pending} className="gap-1.5">
      <Send aria-hidden="true" className="size-3.5" />
      {pending ? "Creating..." : "Create invitation"}
    </Button>
  );
}

export function InvitationCreateForm() {
  const [state, formAction] = useActionState(createInvitationAction, initialMutationState);

  return (
    <form
      action={formAction}
      className="grid gap-2 border-b border-border/70 pb-4 md:grid-cols-[minmax(220px,1fr)_minmax(160px,220px)_minmax(160px,220px)_auto]"
      noValidate
    >
      <Field>
        <FieldLabel htmlFor="invite-email">{tAccountClient("account.fields.email")}</FieldLabel>
        <FieldContent>
          <Input
            id="invite-email"
            name="email"
            type="email"
            autoComplete="email"
            aria-invalid={Boolean(state.fieldErrors?.email?.[0])}
            aria-describedby={state.fieldErrors?.email?.[0] ? "invite-email-error" : undefined}
          />
          <FieldError id="invite-email-error">{state.fieldErrors?.email?.[0]}</FieldError>
        </FieldContent>
      </Field>

      <Field>
        <FieldLabel htmlFor="invite-firstName">
          {tAccountClient("account.fields.firstNameOptional")}
        </FieldLabel>
        <FieldContent>
          <Input id="invite-firstName" name="firstName" />
        </FieldContent>
      </Field>

      <Field>
        <FieldLabel htmlFor="invite-lastName">
          {tAccountClient("account.fields.lastNameOptional")}
        </FieldLabel>
        <FieldContent>
          <Input id="invite-lastName" name="lastName" />
        </FieldContent>
      </Field>

      <div className="flex items-end">
        <SubmitButton />
      </div>
      <div className="md:col-span-4">
        <InvitationActionResult
          state={state}
          successTitle={tAccountClient("account.invitations.created")}
          errorTitle={tAccountClient("account.invitations.createFailed")}
        />
      </div>
    </form>
  );
}
