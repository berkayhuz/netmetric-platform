"use client";

import { useEffect, useRef } from "react";
import { toast } from "@netmetric/ui/client";
import { tCrm } from "@/lib/i18n/crm-i18n";

import type { CrmMutationState } from "@/features/shared/actions/mutation-state";

export function CrmMutationResult({
  locale,
  state,
}: Readonly<{ locale?: string | null | undefined; state: CrmMutationState }>) {
  const lastStateRef = useRef<CrmMutationState | null>(null);

  useEffect(() => {
    if (state.status === "idle" || lastStateRef.current === state) {
      return;
    }
    lastStateRef.current = state;

    if (state.status === "success") {
      toast.success(tCrm("crm.forms.result.completedTitle", locale), {
        description: state.message ?? tCrm("crm.forms.result.completedDescription", locale),
      });
      return;
    }

    toast.error(tCrm("crm.forms.result.errorTitle", locale), {
      description: state.message ?? tCrm("crm.forms.result.tryAgain", locale),
    });
  }, [locale, state]);

  return null;
}
