"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import {
  crmApiClient,
  type ProductBundleLineInput,
  type ProposalTemplateRequest,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { assertSameOriginRequest } from "@/lib/security/csrf";

function revalidateQuoteRoutes(quoteId: string) {
  revalidatePath("/quotes");
  revalidatePath(`/quotes/${quoteId}`);
}

function readOptionalString(formData: FormData, field: string): string | null {
  const value = formData.get(field);
  return typeof value === "string" ? emptyToNull(value) : null;
}

function readRequiredString(formData: FormData, field: string): string {
  const value = readOptionalString(formData, field);
  if (!value) {
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

function readRequiredInteger(formData: FormData, field: string): number {
  const parsed = Number(formData.get(field));
  if (!Number.isInteger(parsed)) {
    throw new Error(`Invalid ${field}.`);
  }

  return parsed;
}

function readOptionalInteger(formData: FormData, field: string): number | null {
  const value = readOptionalString(formData, field);
  if (!value) {
    return null;
  }

  const parsed = Number(value);
  if (!Number.isInteger(parsed)) {
    throw new Error(`Invalid ${field}.`);
  }

  return parsed;
}

function readOptionalGuid(formData: FormData, field: string): string | null {
  const value = readOptionalString(formData, field);
  if (value && !isGuid(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readStringList(formData: FormData, field: string): string[] {
  const value = readOptionalString(formData, field);
  if (!value) {
    return [];
  }

  return value
    .split(/[\n,;]+/g)
    .map((item) => item.trim())
    .filter(Boolean);
}

function readProposalTemplatePayload(formData: FormData): ProposalTemplateRequest {
  return {
    name: readRequiredString(formData, "name"),
    subjectTemplate: readOptionalString(formData, "subjectTemplate"),
    bodyTemplate: readRequiredString(formData, "bodyTemplate"),
    isDefault: formData.get("isDefault") === "true",
    isActive: formData.get("isActive") !== "false",
    notes: readOptionalString(formData, "notes"),
  };
}

function readBundleItems(formData: FormData): ProductBundleLineInput[] {
  const itemsJson = readOptionalString(formData, "itemsJson");
  if (itemsJson) {
    const parsed = JSON.parse(itemsJson) as ProductBundleLineInput[];
    if (
      !Array.isArray(parsed) ||
      parsed.length === 0 ||
      parsed.some((item) => !isGuid(item.productId) || item.quantity <= 0)
    ) {
      throw new Error("Invalid product bundle items.");
    }

    return parsed.map((item) => ({
      productId: item.productId,
      quantity: item.quantity,
      isOptional: Boolean(item.isOptional),
    }));
  }

  const productId = readOptionalString(formData, "productId");
  if (!productId || !isGuid(productId)) {
    throw new Error("Invalid product bundle item.");
  }

  return [
    {
      productId,
      quantity: readRequiredInteger(formData, "quantity"),
      isOptional: formData.get("isOptional") === "true",
    },
  ];
}

export async function runGuidedSellingFormAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");

  if (!isGuid(quoteId)) {
    return { status: "error", message: "Invalid quote id." };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const recommendations = await crmApiClient.runGuidedSelling(
      {
        segment: readOptionalString(formData, "segment"),
        industry: readOptionalString(formData, "industry"),
        budget: readOptionalDecimal(formData, "budget"),
        requiredCapabilities: readStringList(formData, "requiredCapabilities"),
      },
      options,
    );

    const topRecommendations = recommendations
      .slice(0, 3)
      .map((item) => `${item.bundleCode} (${item.score})`)
      .join(", ");

    return {
      status: "success",
      message: topRecommendations
        ? `Guided selling recommendations: ${topRecommendations}.`
        : "Guided selling completed with no matching recommendations.",
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/quotes/${quoteId}`);
  }
}

export async function createProposalTemplateFormAction(
  quoteId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "proposals.manage");

  if (!isGuid(quoteId)) {
    throw new Error("Invalid quote id.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.createProposalTemplate(readProposalTemplatePayload(formData), options);

  revalidateQuoteRoutes(quoteId);
  redirect(`/quotes/${quoteId}`);
}

export async function updateProposalTemplateFormAction(
  quoteId: string,
  templateId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "proposals.manage");

  if (!isGuid(quoteId) || !isGuid(templateId)) {
    throw new Error("Invalid proposal template request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.updateProposalTemplate(
    templateId,
    readProposalTemplatePayload(formData),
    options,
  );

  revalidateQuoteRoutes(quoteId);
  redirect(`/quotes/${quoteId}`);
}

export async function deleteProposalTemplateFormAction(
  quoteId: string,
  templateId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "proposals.manage");

  if (!isGuid(quoteId) || !isGuid(templateId) || formData.get("confirm") !== "delete-template") {
    throw new Error("Invalid proposal template delete request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.deleteProposalTemplate(templateId, options);

  revalidateQuoteRoutes(quoteId);
  redirect(`/quotes/${quoteId}`);
}

export async function upsertGuidedSellingPlaybookFormAction(
  quoteId: string,
  playbookId: string | null,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");

  if (!isGuid(quoteId) || (playbookId && !isGuid(playbookId))) {
    throw new Error("Invalid guided selling playbook request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.upsertGuidedSellingPlaybook(
    playbookId,
    {
      name: readRequiredString(formData, "name"),
      segment: readOptionalString(formData, "segment"),
      industry: readOptionalString(formData, "industry"),
      minimumBudget: readOptionalDecimal(formData, "minimumBudget"),
      maximumBudget: readOptionalDecimal(formData, "maximumBudget"),
      requiredCapabilities: readOptionalString(formData, "requiredCapabilities"),
      recommendedBundleCodes: readStringList(formData, "recommendedBundleCodes"),
      qualificationJson: readOptionalString(formData, "qualificationJson"),
      rowVersion: readOptionalString(formData, "rowVersion"),
    },
    options,
  );

  revalidateQuoteRoutes(quoteId);
  redirect(`/quotes/${quoteId}`);
}

export async function upsertProductBundleFormAction(
  quoteId: string,
  bundleId: string | null,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");

  if (!isGuid(quoteId) || (bundleId && !isGuid(bundleId))) {
    throw new Error("Invalid product bundle request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.upsertProductBundle(
    bundleId,
    {
      code: readRequiredString(formData, "code"),
      name: readRequiredString(formData, "name"),
      description: readOptionalString(formData, "description"),
      segment: readOptionalString(formData, "segment"),
      industry: readOptionalString(formData, "industry"),
      discountRate: readOptionalDecimal(formData, "discountRate") ?? 0,
      minimumBudget: readOptionalDecimal(formData, "minimumBudget"),
      items: readBundleItems(formData),
      rowVersion: readOptionalString(formData, "rowVersion"),
    },
    options,
  );

  revalidateQuoteRoutes(quoteId);
  redirect(`/quotes/${quoteId}`);
}

export async function upsertProductRuleFormAction(
  quoteId: string,
  ruleId: string | null,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");

  if (!isGuid(quoteId) || (ruleId && !isGuid(ruleId))) {
    throw new Error("Invalid product rule request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.upsertProductRule(
    ruleId,
    {
      name: readRequiredString(formData, "name"),
      ruleType: readRequiredString(formData, "ruleType"),
      triggerProductId: readOptionalGuid(formData, "triggerProductId"),
      targetProductId: readOptionalGuid(formData, "targetProductId"),
      minimumQuantity: readOptionalInteger(formData, "minimumQuantity"),
      maximumDiscountRate: readOptionalDecimal(formData, "maximumDiscountRate"),
      severity: readRequiredString(formData, "severity"),
      message: readRequiredString(formData, "message"),
      criteriaJson: readOptionalString(formData, "criteriaJson"),
      rowVersion: readOptionalString(formData, "rowVersion"),
    },
    options,
  );

  revalidateQuoteRoutes(quoteId);
  redirect(`/quotes/${quoteId}`);
}
