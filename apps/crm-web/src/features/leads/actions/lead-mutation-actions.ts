"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type LeadUpdateRequest, type LeadUpsertRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { leadFormSchema, type LeadFormInput, type LeadFormValues } from "../forms/lead-form-schema";

function mapLeadPayload(input: LeadFormValues): LeadUpsertRequest {
  const estimatedBudget = input.estimatedBudget ? Number(input.estimatedBudget) : null;

  return {
    fullName: input.fullName.trim(),
    companyName: emptyToNull(input.companyName),
    email: emptyToNull(input.email),
    phone: emptyToNull(input.phone),
    jobTitle: emptyToNull(input.jobTitle),
    description: emptyToNull(input.description),
    estimatedBudget:
      estimatedBudget === null || Number.isNaN(estimatedBudget) ? null : estimatedBudget,
    nextContactDate: emptyToNull(input.nextContactDate),
    source: input.source,
    status: input.status,
    priority: input.priority,
    companyId: emptyToNull(input.companyId),
    ownerUserId: emptyToNull(input.ownerUserId),
    notes: emptyToNull(input.notes),
  };
}

function mapLeadUpdatePayload(input: LeadFormValues): LeadUpdateRequest {
  return {
    ...mapLeadPayload(input),
    rowVersion: emptyToNull(input.rowVersion),
  };
}

function mapZodErrors(
  fieldErrors: Record<string, string[] | undefined>,
  locale: string,
): Record<string, string[]> {
  return Object.fromEntries(
    Object.entries(fieldErrors).flatMap(([key, errors]) => {
      if (!errors || errors.length === 0) {
        return [];
      }

      return [[key, [tCrm("crm.leads.validation.invalid", locale)]] as const];
    }),
  );
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

export async function createLeadAction(input: LeadFormInput): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/leads/new", "leads.create");
  const locale = await getRequestLocale();

  const parsed = leadFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createLead(mapLeadPayload(parsed.data), options);

    revalidatePath("/leads");
    revalidatePath(`/leads/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.leads.result.created", locale),
      redirectTo: `/leads/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/leads/new");
  }
}

export async function updateLeadAction(
  leadId: string,
  input: LeadFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.edit");
  const locale = await getRequestLocale();

  if (!isGuid(leadId)) {
    return {
      status: "error",
      message: tCrm("crm.leads.validation.invalidId", locale),
    };
  }

  const parsed = leadFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateLead(leadId, mapLeadUpdatePayload(parsed.data), options);

    revalidatePath("/leads");
    revalidatePath(`/leads/${leadId}`);
    revalidatePath(`/leads/${leadId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.leads.result.updated", locale),
      redirectTo: `/leads/${leadId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/leads/${leadId}/edit`);
  }
}

export async function deleteLeadAction(
  leadId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.delete");
  const locale = await getRequestLocale();

  if (!isGuid(leadId)) {
    return { status: "error", message: tCrm("crm.leads.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "delete-lead") {
    return { status: "error", message: tCrm("crm.delete.invalidConfirmation", locale) };
  }

  const confirmText = formData.get("confirmText");
  if (typeof confirmText !== "string" || confirmText.trim().length === 0) {
    return {
      status: "error",
      message: tCrm("crm.delete.typeNameRequired", locale),
      fieldErrors: { confirmText: [tCrm("crm.delete.confirmationRequired", locale)] },
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteLead(leadId, options);

    revalidatePath("/leads");
    redirect("/leads");
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, `/leads/${leadId}`);
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: tCrm("crm.leads.result.alreadyRemoved", locale),
      };
    }

    return mapped;
  }
}

export async function changeLeadStatusFormAction(
  leadId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.edit");

  const status = Number(formData.get("status"));
  if (!isGuid(leadId) || !Number.isInteger(status)) {
    throw new Error("Invalid lead status request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.changeLeadStatus(leadId, { status }, options);

  revalidatePath("/leads");
  revalidatePath(`/leads/${leadId}`);
  redirect(`/leads/${leadId}`);
}

export async function scheduleLeadNextContactFormAction(
  leadId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.edit");

  const rawNextContactDate = formData.get("nextContactDate");
  const nextContactDate =
    typeof rawNextContactDate === "string" && rawNextContactDate.trim().length > 0
      ? rawNextContactDate
      : null;

  if (!isGuid(leadId)) {
    throw new Error("Invalid lead next contact request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.scheduleLeadNextContact(leadId, { nextContactDate }, options);

  revalidatePath("/leads");
  revalidatePath(`/leads/${leadId}`);
  redirect(`/leads/${leadId}`);
}

export async function upsertLeadScoreFormAction(leadId: string, formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.edit");

  const score = Number(formData.get("score"));
  const scoreReason = formData.get("scoreReason");

  if (!isGuid(leadId) || Number.isNaN(score)) {
    throw new Error("Invalid lead score request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.upsertLeadScore(
    leadId,
    {
      score,
      scoreReason: typeof scoreReason === "string" ? emptyToNull(scoreReason) : null,
    },
    options,
  );

  revalidatePath("/leads");
  revalidatePath(`/leads/${leadId}`);
  redirect(`/leads/${leadId}`);
}

export async function upsertLeadQualificationFormAction(
  leadId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.edit");

  const frameworkType = readRequiredInteger(formData, "frameworkType");
  const qualificationDataJson = readOptionalString(formData, "qualificationDataJson") ?? "{}";

  if (!isGuid(leadId)) {
    throw new Error("Invalid lead qualification request.");
  }

  try {
    JSON.parse(qualificationDataJson) as unknown;
  } catch {
    throw new Error("Invalid lead qualification JSON.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.upsertLeadQualification(
    leadId,
    {
      frameworkType,
      qualificationDataJson,
    },
    options,
  );

  revalidatePath("/leads");
  revalidatePath(`/leads/${leadId}`);
  redirect(`/leads/${leadId}`);
}

export async function convertLeadToCustomerFormAction(
  leadId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.edit");

  if (!isGuid(leadId)) {
    throw new Error("Invalid lead conversion request.");
  }

  const companyId = readOptionalString(formData, "companyId");
  if (companyId && !isGuid(companyId)) {
    throw new Error("Invalid lead conversion company.");
  }

  const options = await getCrmApiRequestOptions();
  const result = await crmApiClient.convertLeadToCustomer(
    leadId,
    {
      customerType: readRequiredInteger(formData, "customerType"),
      markCustomerAsVip: formData.get("markCustomerAsVip") === "true",
      createOpportunity: formData.get("createOpportunity") === "true",
      opportunityName: readOptionalString(formData, "opportunityName"),
      estimatedAmount: readOptionalDecimal(formData, "estimatedAmount"),
      companyId,
    },
    options,
  );

  revalidatePath("/leads");
  revalidatePath(`/leads/${leadId}`);
  revalidatePath("/customers");
  revalidatePath(`/customers/${result.customerId}`);
  if (result.opportunityId) {
    revalidatePath("/opportunities");
    revalidatePath(`/opportunities/${result.opportunityId}`);
  }

  redirect(`/customers/${result.customerId}`);
}

export async function assignLeadOwnerFormAction(leadId: string, formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/leads/${leadId}`, "leads.edit");

  const ownerUserId = formData.get("ownerUserId");
  const normalizedOwnerUserId = typeof ownerUserId === "string" ? emptyToNull(ownerUserId) : null;

  if (!isGuid(leadId) || (normalizedOwnerUserId && !isGuid(normalizedOwnerUserId))) {
    throw new Error("Invalid lead owner request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.assignLeadOwner(leadId, { ownerUserId: normalizedOwnerUserId }, options);

  revalidatePath("/leads");
  revalidatePath(`/leads/${leadId}`);
  redirect(`/leads/${leadId}`);
}

export async function bulkAssignLeadsOwnerFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/leads", "leads.edit");

  const ownerUserId = readOptionalString(formData, "ownerUserId");
  if (ownerUserId && !isGuid(ownerUserId)) {
    throw new Error("Invalid lead owner request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.bulkAssignLeadsOwner(
    {
      leadIds: readGuidList(formData, "leadIds"),
      ownerUserId,
    },
    options,
  );

  revalidatePath("/leads");
  redirect("/leads");
}

export async function bulkDeleteLeadsFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/leads", "leads.delete");

  if (formData.get("confirm") !== "bulk-delete-leads") {
    throw new Error("Invalid lead bulk delete confirmation.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.bulkDeleteLeads(
    {
      leadIds: readGuidList(formData, "leadIds"),
    },
    options,
  );

  revalidatePath("/leads");
  redirect("/leads");
}

export async function deleteLeadsBulkFromListAction(leadIds: string[]): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/leads", "leads.delete");
  const locale = await getRequestLocale();

  const uniqueIds = [...new Set(leadIds)].filter((id) => isGuid(id));
  if (uniqueIds.length === 0) {
    return {
      status: "error",
      message: tCrm("crm.leads.validation.invalidId", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.bulkDeleteLeads({ leadIds: uniqueIds }, options);
    revalidatePath("/leads");
    return {
      status: "success",
      message: `${uniqueIds.length} lead(s) deleted.`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/leads");
  }
}
