"use client";

import type { MutationState } from "../actions/mutation-state";
import { SecurityActionResult } from "./security-action-result";

type InvitationActionResultProps = {
  state: MutationState;
  successTitle: string;
  errorTitle: string;
};

export function InvitationActionResult({
  state,
  successTitle,
  errorTitle,
}: InvitationActionResultProps) {
  return <SecurityActionResult state={state} successTitle={successTitle} errorTitle={errorTitle} />;
}
