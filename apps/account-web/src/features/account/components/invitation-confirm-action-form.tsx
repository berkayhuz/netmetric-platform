"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Text } from "@netmetric/ui";

import { initialMutationState, type MutationState } from "../actions/mutation-state";
import { InvitationActionResult } from "./invitation-action-result";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type InvitationConfirmActionFormProps = {
  invitationId: string;
  confirmValue: "resend-invitation" | "revoke-invitation";
  label: string;
  pendingLabel: string;
  variant?: "outline" | "destructive";
  successTitle: string;
  errorTitle: string;
  action: (state: MutationState, formData: FormData) => Promise<MutationState>;
};

function SubmitButton({
  label,
  pendingLabel,
  variant = "outline",
}: {
  label: string;
  pendingLabel: string;
  variant: "outline" | "destructive" | undefined;
}) {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" variant={variant} size="sm" disabled={pending}>
      {pending ? pendingLabel : label}
    </Button>
  );
}

export function InvitationConfirmActionForm({
  invitationId,
  confirmValue,
  label,
  pendingLabel,
  variant,
  successTitle,
  errorTitle,
  action,
}: InvitationConfirmActionFormProps) {
  const [state, formAction] = useActionState(action, initialMutationState);

  return (
    <form action={formAction} className="space-y-2">
      <input type="hidden" name="invitationId" value={invitationId} />
      <input type="hidden" name="confirm" value={confirmValue} />
      <Text className="text-xs text-muted-foreground">
        {tAccountClient("account.invitations.confirmRequired")}
      </Text>
      <SubmitButton label={label} pendingLabel={pendingLabel} variant={variant} />
      <InvitationActionResult state={state} successTitle={successTitle} errorTitle={errorTitle} />
    </form>
  );
}
