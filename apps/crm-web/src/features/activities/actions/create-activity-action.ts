"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { crmApiClient, type CreateActivityRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

type CreateActivityActionInput = {
  primaryRecord: {
    entityType:
      | "lead"
      | "opportunity"
      | "deal"
      | "quote"
      | "ticket"
      | "customer"
      | "company"
      | "contact";
    entityId: string;
  };
  type: "note" | "call" | "email";
  title?: string | null;
  description?: string | null;
  noteBody?: string | null;
  callDirection?: "inbound" | "outbound" | null;
  callOutcome?: "connected" | "no_answer" | "voicemail" | "other" | null;
  callDurationSeconds?: number | null;
  callSummary?: string | null;
  emailSubject?: string | null;
  emailBodySummary?: string | null;
  emailDirection?: "inbound" | "outbound" | null;
  emailTo?: string[] | null;
  emailCc?: string[] | null;
};

const supportedTypes = new Set<CreateActivityActionInput["type"]>(["note", "call", "email"]);

function normalizeText(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  const normalized = value.trim();
  return normalized.length > 0 ? normalized : null;
}

function toValidationError(message: string, field: string): CrmMutationState {
  return {
    status: "error",
    message,
    fieldErrors: {
      [field]: [message],
    },
  };
}

function validateInput(input: CreateActivityActionInput, locale: string): CrmMutationState | null {
  if (!isGuid(input.primaryRecord.entityId)) {
    return toValidationError(
      tCrm("crm.activities.validation.invalidPrimaryRecord", locale),
      "entityId",
    );
  }

  if (!supportedTypes.has(input.type)) {
    return toValidationError(tCrm("crm.activities.validation.invalidType", locale), "type");
  }

  switch (input.type) {
    case "note": {
      if (!normalizeText(input.noteBody)) {
        return toValidationError(
          tCrm("crm.activities.validation.noteBodyRequired", locale),
          "noteBody",
        );
      }

      return null;
    }
    case "call": {
      if (!input.callDirection) {
        return toValidationError(
          tCrm("crm.activities.validation.callDirectionRequired", locale),
          "callDirection",
        );
      }

      if (!input.callOutcome) {
        return toValidationError(
          tCrm("crm.activities.validation.callOutcomeRequired", locale),
          "callOutcome",
        );
      }

      if (input.callDurationSeconds != null && input.callDurationSeconds < 0) {
        return toValidationError(
          tCrm("crm.activities.validation.callDurationInvalid", locale),
          "callDurationSeconds",
        );
      }

      return null;
    }
    case "email": {
      if (!normalizeText(input.emailSubject)) {
        return toValidationError(
          tCrm("crm.activities.validation.emailSubjectRequired", locale),
          "emailSubject",
        );
      }

      if (!normalizeText(input.emailBodySummary)) {
        return toValidationError(
          tCrm("crm.activities.validation.emailBodySummaryRequired", locale),
          "emailBodySummary",
        );
      }

      if (!input.emailDirection) {
        return toValidationError(
          tCrm("crm.activities.validation.emailDirectionRequired", locale),
          "emailDirection",
        );
      }

      return null;
    }
  }
}

function buildPayload(input: CreateActivityActionInput): CreateActivityRequest["payload"] {
  switch (input.type) {
    case "note":
      return {
        body: normalizeText(input.noteBody) ?? "",
      };
    case "call":
      return {
        direction: input.callDirection ?? "outbound",
        outcome: input.callOutcome ?? "other",
        ...(input.callDurationSeconds != null
          ? { durationSeconds: input.callDurationSeconds }
          : {}),
        ...(normalizeText(input.callSummary) ? { summary: normalizeText(input.callSummary) } : {}),
      };
    case "email":
      return {
        subject: normalizeText(input.emailSubject) ?? "",
        bodySummary: normalizeText(input.emailBodySummary) ?? "",
        direction: input.emailDirection ?? "outbound",
        ...(input.emailTo && input.emailTo.length > 0 ? { to: input.emailTo } : {}),
        ...(input.emailCc && input.emailCc.length > 0 ? { cc: input.emailCc } : {}),
      };
  }
}

export async function createActivityAction(
  input: CreateActivityActionInput,
): Promise<CrmMutationState> {
  const returnPath = `/${input.primaryRecord.entityType}s/${input.primaryRecord.entityId}`;
  await assertSameOriginRequest();
  await requireCrmSession(returnPath);
  await requireCrmActionCapability(returnPath, "activities.create");

  const locale = await getRequestLocale();
  const validationError = validateInput(input, locale);
  if (validationError) {
    return validationError;
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.createActivity(
      {
        type: input.type,
        title: normalizeText(input.title),
        description: normalizeText(input.description),
        relatedRecords: [
          {
            entityType: input.primaryRecord.entityType,
            entityId: input.primaryRecord.entityId,
            relationRole: "primary",
          },
        ],
        payload: buildPayload(input),
      },
      options,
    );

    revalidatePath("/activities");
    revalidatePath(returnPath);

    return {
      status: "success",
      message: tCrm("crm.activities.actions.created", locale),
    };
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, returnPath);
    return {
      ...mapped,
      message: mapped.message || tCrm("crm.activities.actions.createFailed", locale),
    };
  }
}
