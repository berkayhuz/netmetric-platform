import "server-only";

import { redirect } from "next/navigation";

import { CrmApiError } from "@/lib/crm-api";
import { resolveActionErrorMessage } from "@netmetric/i18n";
import { buildAuthLoginRedirectUrl } from "@/lib/crm-auth/safe-return-url";
import { tCrm } from "@/lib/i18n/crm-i18n";

import type { CrmMutationState } from "./mutation-state";

function normalizeFieldKey(key: string): string {
  if (!key) {
    return key;
  }

  return key.charAt(0).toLowerCase() + key.slice(1);
}

function mapFieldErrors(error: CrmApiError): Record<string, string[]> | undefined {
  const source = error.problem?.errors;
  if (!source) {
    return undefined;
  }

  return Object.fromEntries(
    Object.entries(source).map(([key, values]) => [normalizeFieldKey(key), values]),
  );
}

export function mapCrmMutationErrorToState(error: unknown, returnPath: string): CrmMutationState {
  const tContract = (key: string) => tCrm(key);

  if (error instanceof CrmApiError) {
    if (error.kind === "unauthorized") {
      redirect(buildAuthLoginRedirectUrl(returnPath));
    }

    if (error.kind === "forbidden") {
      redirect("/access-denied");
    }

    if (error.kind === "rate_limited") {
      redirect("/retry-later");
    }

    if (error.kind === "server_error" || error.kind === "upstream_unavailable") {
      redirect("/service-unavailable");
    }

    if (error.kind === "validation") {
      const fieldErrors = mapFieldErrors(error);
      if (fieldErrors) {
        return {
          status: "error",
          message:
            error.problem?.detail ??
            resolveActionErrorMessage(
              tContract,
              "validation",
              tCrm("crm.forms.errors.reviewTitle"),
            ),
          fieldErrors,
        };
      }

      return {
        status: "error",
        message:
          error.problem?.detail ??
          resolveActionErrorMessage(tContract, "validation", tCrm("crm.forms.errors.reviewTitle")),
      };
    }

    if (error.kind === "conflict") {
      return {
        status: "error",
        message:
          error.problem?.detail ??
          resolveActionErrorMessage(tContract, "conflict", tCrm("crm.forms.errors.conflict")),
      };
    }

    if (error.kind === "not_found") {
      return {
        status: "error",
        message: resolveActionErrorMessage(
          tContract,
          "not_found",
          tCrm("crm.forms.errors.notFound"),
        ),
      };
    }

    return {
      status: "error",
      message: resolveActionErrorMessage(
        tContract,
        "server_error",
        tCrm("crm.forms.errors.saveFailed"),
      ),
    };
  }

  return {
    status: "error",
    message: resolveActionErrorMessage(
      tContract,
      "unknown",
      tCrm("crm.forms.errors.unexpectedSave"),
    ),
  };
}
