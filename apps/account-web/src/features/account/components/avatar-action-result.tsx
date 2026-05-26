"use client";

import type { MutationState } from "../actions/mutation-state";
import { SecurityActionResult } from "./security-action-result";

type AvatarActionResultProps = {
  state: MutationState;
  successTitle: string;
  errorTitle: string;
};

export function AvatarActionResult({ state, successTitle, errorTitle }: AvatarActionResultProps) {
  return <SecurityActionResult state={state} successTitle={successTitle} errorTitle={errorTitle} />;
}
