"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from "@netmetric/ui";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

function SubmitButton({ disabled }: Readonly<{ disabled: boolean }>) {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" disabled={disabled || pending} aria-busy={pending}>
      {pending
        ? tCrmClient("crm.forms.actions.processing")
        : tCrmClient("crm.contacts.actions.setPrimary")}
    </Button>
  );
}

export function ContactDetailActions({
  isPrimaryContact,
  setPrimaryAction,
}: Readonly<{
  isPrimaryContact: boolean;
  setPrimaryAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
}>) {
  const [state, formAction] = useActionState(setPrimaryAction, initialCrmMutationState);

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrmClient("crm.contacts.pages.detail.actionsTitle")}</CardTitle>
        <CardDescription>
          {tCrmClient("crm.contacts.pages.detail.actionsDescription")}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <CrmMutationResult state={state} />
        <form action={formAction}>
          <SubmitButton disabled={isPrimaryContact} />
        </form>
      </CardContent>
    </Card>
  );
}
