"use client";

import { SecurityActionResult } from "./security-action-result";
import type { MutationState } from "../actions/mutation-state";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type ConsentActionResultProps = {
  state: MutationState;
};

export function ConsentActionResult({ state }: ConsentActionResultProps) {
  return (
    <SecurityActionResult
      state={state}
      successTitle={tAccountClient("account.privacy.consentAccepted")}
      errorTitle={tAccountClient("account.privacy.consentAcceptFailed")}
    />
  );
}
