"use client";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";

import type { CrmMutationState } from "@/features/shared/actions/mutation-state";

export function CrmDeleteActionResult({ state }: Readonly<{ state: CrmMutationState }>) {
  return <CrmMutationResult state={state} />;
}
