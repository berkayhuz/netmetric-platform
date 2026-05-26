"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { assertSameOriginRequest } from "@/lib/security/csrf";

function revalidateOpportunityRoutes(opportunityId: string) {
  revalidatePath("/opportunities");
  revalidatePath("/pipeline");
  revalidatePath(`/opportunities/${opportunityId}`);
}

function readOptionalString(formData: FormData, field: string): string | null {
  const value = formData.get(field);
  return typeof value === "string" ? emptyToNull(value) : null;
}

function readRequiredInteger(formData: FormData, field: string): number {
  const value = Number(formData.get(field));
  if (!Number.isInteger(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
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

function readRequiredDecimal(formData: FormData, field: string): number {
  const parsed = readOptionalDecimal(formData, field);
  if (parsed === null) {
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

function readRequiredGuid(formData: FormData, field: string): string {
  const value = readOptionalString(formData, field);
  if (!value || !isGuid(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

export async function assignOpportunityOwnerFormAction(
  opportunityId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.edit");

  const ownerUserId = readOptionalString(formData, "ownerUserId");
  if (!isGuid(opportunityId) || (ownerUserId && !isGuid(ownerUserId))) {
    throw new Error("Invalid opportunity owner request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.assignOpportunityOwner(opportunityId, { ownerUserId }, options);

  revalidateOpportunityRoutes(opportunityId);
  redirect(`/opportunities/${opportunityId}`);
}

export async function changeOpportunityStageFormAction(
  opportunityId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.edit");

  if (!isGuid(opportunityId)) {
    throw new Error("Invalid opportunity stage request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.changeOpportunityStage(
    opportunityId,
    {
      newStage: readRequiredInteger(formData, "newStage"),
      note: readOptionalString(formData, "note"),
      rowVersion: readOptionalString(formData, "rowVersion"),
    },
    options,
  );

  revalidateOpportunityRoutes(opportunityId);
  redirect(`/opportunities/${opportunityId}`);
}

export async function markOpportunityWonFormAction(
  opportunityId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.edit");

  if (!isGuid(opportunityId)) {
    throw new Error("Invalid opportunity win request.");
  }

  const options = await getCrmApiRequestOptions();
  const result = await crmApiClient.markOpportunityWon(
    opportunityId,
    {
      dealName: readOptionalString(formData, "dealName"),
      closedDate: readOptionalString(formData, "closedDate") ?? new Date().toISOString(),
      rowVersion: readOptionalString(formData, "rowVersion"),
    },
    options,
  );

  revalidateOpportunityRoutes(opportunityId);
  revalidatePath("/deals");
  revalidatePath(`/deals/${result.dealId}`);
  redirect(`/deals/${result.dealId}`);
}

export async function markOpportunityLostFormAction(
  opportunityId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.edit");

  const lostReasonId = readOptionalString(formData, "lostReasonId");
  if (!isGuid(opportunityId) || (lostReasonId && !isGuid(lostReasonId))) {
    throw new Error("Invalid opportunity loss request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.markOpportunityLost(
    opportunityId,
    {
      lostReasonId,
      lostNote: readOptionalString(formData, "lostNote"),
      rowVersion: readOptionalString(formData, "rowVersion"),
    },
    options,
  );

  revalidateOpportunityRoutes(opportunityId);
  redirect(`/opportunities/${opportunityId}`);
}

export async function addOpportunityContactFormAction(
  opportunityId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.edit");

  if (!isGuid(opportunityId)) {
    throw new Error("Invalid opportunity contact request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.addOpportunityContact(
    opportunityId,
    {
      contactId: readRequiredGuid(formData, "contactId"),
      isDecisionMaker: formData.get("isDecisionMaker") === "true",
      isPrimary: formData.get("isPrimary") === "true",
    },
    options,
  );

  revalidateOpportunityRoutes(opportunityId);
  redirect(`/opportunities/${opportunityId}`);
}

export async function addOpportunityProductFormAction(
  opportunityId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.edit");

  if (!isGuid(opportunityId)) {
    throw new Error("Invalid opportunity product request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.addOpportunityProduct(
    opportunityId,
    {
      productId: readRequiredGuid(formData, "productId"),
      quantity: readRequiredInteger(formData, "quantity"),
      unitPrice: readRequiredDecimal(formData, "unitPrice"),
      discountRate: readOptionalDecimal(formData, "discountRate") ?? 0,
      vatRate: readOptionalDecimal(formData, "vatRate") ?? 0,
    },
    options,
  );

  revalidateOpportunityRoutes(opportunityId);
  redirect(`/opportunities/${opportunityId}`);
}

export async function createOpportunityQuoteFormAction(
  opportunityId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunityQuotes.manage");

  if (!isGuid(opportunityId)) {
    throw new Error("Invalid opportunity quote request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.createOpportunityQuote(
    opportunityId,
    {
      quoteNumber: readOptionalString(formData, "quoteNumber") ?? `Q-${Date.now()}`,
      quoteDate: readOptionalString(formData, "quoteDate") ?? new Date().toISOString(),
      validUntil: readOptionalString(formData, "validUntil"),
      termsAndConditions: readOptionalString(formData, "termsAndConditions"),
      ownerUserId: readOptionalString(formData, "ownerUserId"),
      currencyCode: readOptionalString(formData, "currencyCode") ?? "TRY",
      exchangeRate: readOptionalDecimal(formData, "exchangeRate") ?? 1,
      items: [
        {
          productId: readRequiredGuid(formData, "productId"),
          description: readOptionalString(formData, "description"),
          quantity: readRequiredInteger(formData, "quantity"),
          unitPrice: readRequiredDecimal(formData, "unitPrice"),
          discountRate: readOptionalDecimal(formData, "discountRate") ?? 0,
          taxRate: readOptionalDecimal(formData, "taxRate") ?? 0,
        },
      ],
    },
    options,
  );

  revalidateOpportunityRoutes(opportunityId);
  revalidatePath("/quotes");
  redirect(`/opportunities/${opportunityId}`);
}

export async function bulkAssignOpportunitiesOwnerFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/opportunities", "opportunities.edit");

  const ownerUserId = readOptionalString(formData, "ownerUserId");
  if (ownerUserId && !isGuid(ownerUserId)) {
    throw new Error("Invalid opportunity owner request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.bulkAssignOpportunitiesOwner(
    {
      opportunityIds: readGuidList(formData, "opportunityIds"),
      ownerUserId,
    },
    options,
  );

  revalidatePath("/opportunities");
  revalidatePath("/pipeline");
  redirect("/opportunities");
}

export async function bulkChangeOpportunitiesStageFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/opportunities", "opportunities.edit");

  const options = await getCrmApiRequestOptions();
  await crmApiClient.bulkChangeOpportunitiesStage(
    {
      opportunityIds: readGuidList(formData, "opportunityIds"),
      newStage: readRequiredInteger(formData, "newStage"),
      note: readOptionalString(formData, "note"),
    },
    options,
  );

  revalidatePath("/opportunities");
  revalidatePath("/pipeline");
  redirect("/opportunities");
}
