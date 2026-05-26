"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, FieldError, Input, Text } from "@netmetric/ui";

import { initialMutationState, type MutationState } from "../actions/mutation-state";
import { AccountSection } from "./account-section";
import { SecurityActionResult } from "./security-action-result";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type EmailChangeConfirmPanelProps = {
  action: (state: MutationState, formData: FormData) => Promise<MutationState>;
  tokenFromQuery: string | undefined;
};

function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <Button size="xs" type="submit" disabled={pending}>
      {pending
        ? tAccountClient("account.security.email.confirming")
        : tAccountClient("account.security.email.confirmSubmit")}
    </Button>
  );
}

export function EmailChangeConfirmPanel({ action, tokenFromQuery }: EmailChangeConfirmPanelProps) {
  const [state, formAction] = useActionState(action, initialMutationState);

  return (
    <AccountSection
      title={tAccountClient("account.security.email.confirmTitle")}
      description={tAccountClient("account.security.email.confirmDescription")}
      contentClassName="space-y-4"
    >
      <Text className="text-sm text-muted-foreground">
        {tAccountClient("account.security.email.confirmHelp")}
      </Text>
      <SecurityActionResult
        state={state}
        successTitle={tAccountClient("account.security.email.confirmed")}
        errorTitle={tAccountClient("account.security.email.confirmFailed")}
      />

      <form action={formAction} className="space-y-4" noValidate>
        {tokenFromQuery ? <input type="hidden" name="token" value={tokenFromQuery} /> : null}
        {!tokenFromQuery ? (
          <>
            <Input
              name="token"
              aria-label={tAccountClient("account.security.email.confirmToken")}
              aria-invalid={Boolean(state.fieldErrors?.token?.[0])}
              aria-describedby={state.fieldErrors?.token?.[0] ? "token-error" : undefined}
            />
            <FieldError id="token-error">{state.fieldErrors?.token?.[0]}</FieldError>
          </>
        ) : null}
        <div className="flex flex-wrap items-center gap-2">
          <SubmitButton />
        </div>
      </form>
    </AccountSection>
  );
}
