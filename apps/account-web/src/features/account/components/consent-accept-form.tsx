"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Text } from "@netmetric/ui";

import { acceptConsentAction } from "../actions/consent-actions";
import { initialMutationState } from "../actions/mutation-state";
import { ConsentActionResult } from "./consent-action-result";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type ConsentAcceptFormProps = {
  consentType: string;
  version: string;
};

function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" size="sm" disabled={pending}>
      {pending ? "Accepting..." : "Accept consent"}
    </Button>
  );
}

export function ConsentAcceptForm({ consentType, version }: ConsentAcceptFormProps) {
  const [state, formAction] = useActionState(acceptConsentAction, initialMutationState);

  return (
    <form action={formAction} className="space-y-2">
      <input type="hidden" name="confirm" value="accept-consent" />
      <input type="hidden" name="consentType" value={consentType} />
      <input type="hidden" name="version" value={version} />
      <Text className="text-xs text-muted-foreground">
        {tAccountClient("account.privacy.confirmConsent")}
      </Text>
      <SubmitButton />
      <ConsentActionResult state={state} />
    </form>
  );
}
