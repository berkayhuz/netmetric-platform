"use client";

import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import type { MutationState } from "../actions/mutation-state";

type SecurityActionResultProps = {
  state: MutationState;
  successTitle: string;
  errorTitle: string;
};

export function SecurityActionResult({
  state,
  successTitle,
  errorTitle,
}: SecurityActionResultProps) {
  if (!state.message || state.status === "idle") {
    return null;
  }

  if (state.status === "success") {
    return (
      <Alert>
        <AlertTitle>{successTitle}</AlertTitle>
        <AlertDescription>{state.message}</AlertDescription>
      </Alert>
    );
  }

  return (
    <Alert variant="destructive">
      <AlertTitle>{errorTitle}</AlertTitle>
      <AlertDescription>{state.message}</AlertDescription>
    </Alert>
  );
}
