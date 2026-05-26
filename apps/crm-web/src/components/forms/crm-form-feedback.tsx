"use client";

import type { CrmMutationState } from "@/features/shared/actions/mutation-state";

import { CrmFormErrorSummary } from "./crm-form-error-summary";
import { CrmMutationResult } from "./crm-mutation-result";

export function CrmFormFeedback({
  state,
  locale,
  summaryTitle,
}: Readonly<{
  state: CrmMutationState;
  locale?: string | null | undefined;
  summaryTitle?: string;
}>) {
  return (
    <>
      <CrmFormErrorSummary
        {...(summaryTitle ? { title: summaryTitle } : {})}
        {...(state.status === "error" && state.message ? { message: state.message } : {})}
        {...(state.fieldErrors ? { errors: state.fieldErrors } : {})}
      />
      <CrmMutationResult state={state} locale={locale} />
    </>
  );
}
