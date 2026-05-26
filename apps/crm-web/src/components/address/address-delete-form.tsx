"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button } from "@netmetric/ui";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { AddressActionResult } from "@/components/address/address-action-result";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";

function SubmitButton() {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" variant="destructive" disabled={pending} aria-busy={pending}>
      {pending
        ? tCrmClient("crm.address.actions.deleting")
        : tCrmClient("crm.address.actions.delete")}
    </Button>
  );
}

export function AddressDeleteForm({
  action,
}: Readonly<{
  action: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
}>) {
  const [state, formAction] = useActionState(action, initialCrmMutationState);

  return (
    <form action={formAction} className="space-y-3" noValidate>
      <input type="hidden" name="confirm" value="delete-address" />
      <AddressActionResult state={state} />
      <SubmitButton />
    </form>
  );
}
