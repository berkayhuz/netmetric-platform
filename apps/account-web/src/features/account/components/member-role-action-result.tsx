"use client";

import type { MutationState } from "../actions/mutation-state";
import { SecurityActionResult } from "./security-action-result";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type MemberRoleActionResultProps = {
  state: MutationState;
};

export function MemberRoleActionResult({ state }: MemberRoleActionResultProps) {
  return (
    <SecurityActionResult
      state={state}
      successTitle={tAccountClient("account.team.rolesUpdated")}
      errorTitle={tAccountClient("account.team.roleUpdateFailed")}
    />
  );
}
