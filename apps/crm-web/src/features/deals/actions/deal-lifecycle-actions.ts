"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type DealOutcomeRequest, type DealReviewUpsertRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { dealLifecycleActionSchema } from "../forms/deal-lifecycle-action-schema";
import { dealOwnerFormSchema } from "../forms/deal-owner-form-schema";

function mapZodErrors(
  fieldErrors: Record<string, string[] | undefined>,
  locale: string,
): Record<string, string[]> {
  return Object.fromEntries(
    Object.entries(fieldErrors).flatMap(([key, errors]) => {
      if (!errors || errors.length === 0) {
        return [];
      }

      return [[key, [tCrm("crm.deals.validation.invalid", locale)]] as const];
    }),
  );
}

function revalidateDealRoutes(dealId: string) {
  revalidatePath("/deals");
  revalidatePath(`/deals/${dealId}`);
}

function readOptionalString(formData: FormData, field: string): string | null {
  const value = formData.get(field);
  return typeof value === "string" ? emptyToNull(value) : null;
}

function readOptionalDecimal(formData: FormData, field: string): number | null {
  const value = readOptionalString(formData, field);
  if (!value) {
    return null;
  }

  const parsed = Number(value);
  if (Number.isNaN(parsed)) {
    throw new Error(`Invalid ${field}.`);
  }

  return parsed;
}

function readGuidList(formData: FormData, field: string): string[] {
  const raw = formData.get(field);
  if (typeof raw !== "string") {
    throw new Error(`Invalid ${field}.`);
  }

  const values = raw
    .split(/[\s,;]+/)
    .map((value) => value.trim())
    .filter(Boolean);

  if (values.length === 0 || values.some((value) => !isGuid(value))) {
    throw new Error(`Invalid ${field}.`);
  }

  return values;
}

function mapLifecyclePayload(formData: FormData): DealOutcomeRequest | null {
  const parsed = dealLifecycleActionSchema.safeParse({
    occurredAt: formData.get("occurredAt"),
    lostReasonId: formData.get("lostReasonId"),
    note: formData.get("note"),
    rowVersion: formData.get("rowVersion"),
  });

  if (!parsed.success) {
    return null;
  }

  return {
    occurredAt: emptyToNull(parsed.data.occurredAt),
    lostReasonId: emptyToNull(parsed.data.lostReasonId),
    note: emptyToNull(parsed.data.note),
    rowVersion: emptyToNull(parsed.data.rowVersion),
  };
}

export async function changeDealOwnerAction(
  dealId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/deals/${dealId}`, "deals.edit");
  const locale = await getRequestLocale();

  if (!isGuid(dealId)) {
    return { status: "error", message: tCrm("crm.deals.validation.invalidId", locale) };
  }

  const parsed = dealOwnerFormSchema.safeParse({
    ownerUserId: formData.get("ownerUserId"),
  });

  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.assignDealOwner(
      dealId,
      { ownerUserId: emptyToNull(parsed.data.ownerUserId) },
      options,
    );

    revalidateDealRoutes(dealId);
    return { status: "success", message: tCrm("crm.deals.result.ownerUpdated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/deals/${dealId}`);
  }
}

export async function bulkAssignDealsOwnerFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/deals", "deals.edit");

  const ownerUserId = readOptionalString(formData, "ownerUserId");
  if (ownerUserId && !isGuid(ownerUserId)) {
    throw new Error("Invalid deal owner request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.bulkAssignDealsOwner(
    {
      dealIds: readGuidList(formData, "dealIds"),
      ownerUserId,
    },
    options,
  );

  revalidatePath("/deals");
  redirect("/deals");
}

export async function markDealWonAction(
  dealId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/deals/${dealId}`, "deals.edit");
  const locale = await getRequestLocale();

  if (!isGuid(dealId)) {
    return { status: "error", message: tCrm("crm.deals.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "mark-deal-won") {
    return {
      status: "error",
      message: tCrm("crm.deals.validation.invalidActionConfirmation", locale),
    };
  }

  const payload = mapLifecyclePayload(formData);
  if (!payload) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.markDealWon(dealId, payload, options);
    revalidateDealRoutes(dealId);
    return { status: "success", message: tCrm("crm.deals.result.markedWon", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/deals/${dealId}`);
  }
}

export async function upsertDealWinLossReviewFormAction(
  dealId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/deals/${dealId}`, "winLoss.manage");

  const outcome = readOptionalString(formData, "outcome");
  if (!isGuid(dealId) || !outcome) {
    throw new Error("Invalid win-loss review request.");
  }

  const payload: DealReviewUpsertRequest = {
    outcome,
    summary: readOptionalString(formData, "summary"),
    strengths: readOptionalString(formData, "strengths"),
    risks: readOptionalString(formData, "risks"),
    competitorName: readOptionalString(formData, "competitorName"),
    competitorPrice: readOptionalDecimal(formData, "competitorPrice"),
    customerFeedback: readOptionalString(formData, "customerFeedback"),
    rowVersion: readOptionalString(formData, "rowVersion"),
  };

  const options = await getCrmApiRequestOptions();
  await crmApiClient.upsertDealWinLossReview(dealId, payload, options);

  revalidateDealRoutes(dealId);
  redirect(`/deals/${dealId}`);
}

export async function markDealLostAction(
  dealId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/deals/${dealId}`, "deals.edit");
  const locale = await getRequestLocale();

  if (!isGuid(dealId)) {
    return { status: "error", message: tCrm("crm.deals.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "mark-deal-lost") {
    return {
      status: "error",
      message: tCrm("crm.deals.validation.invalidActionConfirmation", locale),
    };
  }

  const payload = mapLifecyclePayload(formData);
  if (!payload) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.markDealLost(dealId, payload, options);
    revalidateDealRoutes(dealId);
    return { status: "success", message: tCrm("crm.deals.result.markedLost", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/deals/${dealId}`);
  }
}

export async function reopenDealAction(
  dealId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/deals/${dealId}`, "deals.edit");
  const locale = await getRequestLocale();

  if (!isGuid(dealId)) {
    return { status: "error", message: tCrm("crm.deals.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "reopen-deal") {
    return {
      status: "error",
      message: tCrm("crm.deals.validation.invalidActionConfirmation", locale),
    };
  }

  const payload = mapLifecyclePayload(formData);
  if (!payload) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.reopenDeal(dealId, payload, options);
    revalidateDealRoutes(dealId);
    return { status: "success", message: tCrm("crm.deals.result.reopened", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/deals/${dealId}`);
  }
}
