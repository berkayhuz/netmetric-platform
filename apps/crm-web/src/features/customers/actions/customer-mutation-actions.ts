"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type CustomerUpsertRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  customerFormSchema,
  type CustomerFormInput,
  type CustomerFormValues,
} from "../forms/customer-form-schema";

function mapCustomerPayload(input: CustomerFormValues): CustomerUpsertRequest {
  return {
    firstName: input.firstName.trim(),
    lastName: input.lastName.trim(),
    title: emptyToNull(input.title),
    email: emptyToNull(input.email),
    mobilePhone: emptyToNull(input.mobilePhone),
    workPhone: emptyToNull(input.workPhone),
    personalPhone: emptyToNull(input.personalPhone),
    birthDate: emptyToNull(input.birthDate),
    gender: input.gender,
    department: emptyToNull(input.department),
    jobTitle: emptyToNull(input.jobTitle),
    description: emptyToNull(input.description),
    notes: emptyToNull(input.notes),
    ownerUserId: emptyToNull(input.ownerUserId),
    customerType: input.customerType,
    identityNumber: emptyToNull(input.identityNumber),
    isVip: input.isVip,
    isActive: input.isActive,
    companyId: emptyToNull(input.companyId),
    rowVersion: emptyToNull(input.rowVersion),
  };
}

function localizeFieldErrors(
  fieldErrors: Record<string, string[] | undefined>,
  locale: string,
): Record<string, string[]> {
  return Object.fromEntries(
    Object.entries(fieldErrors).flatMap(([key, errors]) => {
      if (!errors || errors.length === 0) {
        return [];
      }

      const first = errors[0] ?? "";
      const isRequired =
        first.toLowerCase().includes("required") || first.toLowerCase().includes("small");
      const resolved = isRequired
        ? tCrm("crm.customers.validation.required", locale)
        : tCrm("crm.customers.validation.invalid", locale);
      return [[key, [resolved]] as const];
    }),
  );
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

function readRequiredInteger(formData: FormData, field: string): number {
  const value = Number(formData.get(field));
  if (!Number.isInteger(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readOptionalGuid(formData: FormData, field: string): string | null {
  const value = readOptionalString(formData, field);
  if (value && !isGuid(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readRequiredGuid(formData: FormData, field: string): string {
  const value = readOptionalGuid(formData, field);
  if (!value) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function getImageUploadPayload(formData: FormData, locale: string): FormData | CrmMutationState {
  const file = formData.get("file");
  if (!file || typeof file !== "object" || !("size" in file) || Number(file.size) <= 0) {
    return {
      status: "error",
      message: tCrm("crm.media.validation.imageRequired", locale),
      fieldErrors: { file: [tCrm("crm.media.validation.imageRequired", locale)] },
    };
  }

  const image = file as Blob & { name?: string };
  const payload = new FormData();
  payload.set("file", image, image.name ?? "customer-image");
  return payload;
}

function readResolvedFields(formData: FormData): Record<string, string | null> {
  const raw = readOptionalString(formData, "resolvedFieldsJson") ?? "{}";

  try {
    const parsed = JSON.parse(raw) as unknown;
    if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
      throw new Error("Invalid resolved fields.");
    }

    return Object.fromEntries(
      Object.entries(parsed).map(([key, value]) => [
        key,
        typeof value === "string" ? value : value === null ? null : String(value),
      ]),
    );
  } catch {
    throw new Error("Invalid resolved fields JSON.");
  }
}

function readImportRows(formData: FormData): Array<Record<string, string | null>> {
  const raw = readRequiredString(formData, "rowsJson");

  try {
    const parsed = JSON.parse(raw) as unknown;
    if (!Array.isArray(parsed)) {
      throw new Error("Invalid import rows.");
    }

    return parsed.map((row) => {
      if (!row || typeof row !== "object" || Array.isArray(row)) {
        throw new Error("Invalid import row.");
      }

      return Object.fromEntries(
        Object.entries(row).map(([key, value]) => [
          key,
          typeof value === "string" ? value : value === null ? null : String(value),
        ]),
      );
    });
  } catch {
    throw new Error("Invalid import rows JSON.");
  }
}

export async function createCustomerAction(input: CustomerFormInput): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers/new", "customers.create");
  const locale = await getRequestLocale();

  const parsed = customerFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createCustomer(mapCustomerPayload(parsed.data), options);

    revalidatePath("/customers");
    revalidatePath(`/customers/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.customers.result.created", locale),
      redirectTo: `/customers/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/customers/new");
  }
}

export async function updateCustomerAction(
  customerId: string,
  input: CustomerFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");
  const locale = await getRequestLocale();

  if (!isGuid(customerId)) {
    return {
      status: "error",
      message: tCrm("crm.customers.validation.invalidId", locale),
    };
  }

  const parsed = customerFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateCustomer(customerId, mapCustomerPayload(parsed.data), options);

    revalidatePath("/customers");
    revalidatePath(`/customers/${customerId}`);
    revalidatePath(`/customers/${customerId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.customers.result.updated", locale),
      redirectTo: `/customers/${customerId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/customers/${customerId}/edit`);
  }
}

export async function deleteCustomerAction(
  customerId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.delete");
  const locale = await getRequestLocale();

  if (!isGuid(customerId)) {
    return { status: "error", message: tCrm("crm.customers.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "delete-customer") {
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
    await crmApiClient.deleteCustomer(customerId, options);

    revalidatePath("/customers");
    redirect("/customers");
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, `/customers/${customerId}`);
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: tCrm("crm.customers.result.alreadyRemoved", locale),
      };
    }

    return mapped;
  }
}

export async function deleteCustomerFromListAction(customerId: string): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers", "customers.delete");
  const locale = await getRequestLocale();

  if (!isGuid(customerId)) {
    return { status: "error", message: tCrm("crm.customers.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteCustomer(customerId, options);
    revalidatePath("/customers");
    return { status: "success", message: tCrm("crm.customers.result.deleted", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/customers");
  }
}

export async function deleteCustomersBulkFromListAction(
  customerIds: string[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers", "customers.delete");
  const locale = await getRequestLocale();

  const uniqueIds = [...new Set(customerIds)].filter((id) => isGuid(id));
  if (uniqueIds.length === 0) {
    return {
      status: "error",
      message: tCrm("crm.customers.validation.invalidId", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await Promise.all(uniqueIds.map((id) => crmApiClient.deleteCustomer(id, options)));
    revalidatePath("/customers");
    return {
      status: "success",
      message: `${uniqueIds.length} customer(s) deleted.`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/customers");
  }
}

export async function uploadCustomerImageAction(
  customerId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");
  const locale = await getRequestLocale();

  if (!isGuid(customerId)) {
    return { status: "error", message: tCrm("crm.customers.validation.invalidId", locale) };
  }

  const payload = getImageUploadPayload(formData, locale);
  if (!(payload instanceof FormData)) {
    return payload;
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.uploadCustomerImage(customerId, payload, options);

    revalidatePath("/customers");
    revalidatePath(`/customers/${customerId}`);
    return { status: "success", message: tCrm("crm.customers.result.imageUpdated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/customers/${customerId}`);
  }
}

export async function removeCustomerImageAction(
  customerId: string,
  _previous: CrmMutationState,
  _formData: FormData,
): Promise<CrmMutationState> {
  void _previous;
  void _formData;

  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");
  const locale = await getRequestLocale();

  if (!isGuid(customerId)) {
    return { status: "error", message: tCrm("crm.customers.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.removeCustomerImage(customerId, options);

    revalidatePath("/customers");
    revalidatePath(`/customers/${customerId}`);
    return { status: "success", message: tCrm("crm.customers.result.imageRemoved", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/customers/${customerId}`);
  }
}

export async function markCustomerVipFormAction(
  customerId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");

  if (!isGuid(customerId)) {
    throw new Error("Invalid customer VIP request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.markCustomerVip(customerId, formData.get("isVip") === "true", options);

  revalidatePath("/customers");
  revalidatePath(`/customers/${customerId}`);
  redirect(`/customers/${customerId}`);
}

export async function changeCustomerLifecycleStageFormAction(
  customerId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");

  if (!isGuid(customerId)) {
    throw new Error("Invalid customer lifecycle request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.changeCustomerLifecycleStage(
    customerId,
    {
      newStage: readRequiredInteger(formData, "newStage"),
      reason: readOptionalString(formData, "reason"),
    },
    options,
  );

  revalidatePath("/customers");
  revalidatePath(`/customers/${customerId}`);
  redirect(`/customers/${customerId}`);
}

export async function recalculateCustomerDataQualityFormAction(
  customerId: string,
  _formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");
  void _formData;

  if (!isGuid(customerId)) {
    throw new Error("Invalid customer data quality request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.recalculateCustomerDataQuality(customerId, options);

  revalidatePath(`/customers/${customerId}`);
  redirect(`/customers/${customerId}`);
}

export async function recalculateCustomerRelationshipHealthFormAction(
  customerId: string,
  _formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");
  void _formData;

  if (!isGuid(customerId)) {
    throw new Error("Invalid customer relationship health request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.recalculateCustomerRelationshipHealth(customerId, options);

  revalidatePath(`/customers/${customerId}`);
  redirect(`/customers/${customerId}`);
}

export async function upsertCustomerConsentFormAction(
  customerId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");

  if (!isGuid(customerId)) {
    throw new Error("Invalid customer consent request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.upsertCustomerConsent(
    customerId,
    {
      channel: readRequiredInteger(formData, "channel"),
      purpose: readRequiredInteger(formData, "purpose"),
      status: readRequiredInteger(formData, "status"),
      source: readRequiredInteger(formData, "source"),
      validUntilUtc: readOptionalString(formData, "validUntilUtc"),
      evidenceText: readOptionalString(formData, "evidenceText"),
      evidenceIpAddress: readOptionalString(formData, "evidenceIpAddress"),
      evidenceUserAgent: readOptionalString(formData, "evidenceUserAgent"),
      reason: readOptionalString(formData, "reason"),
    },
    options,
  );

  revalidatePath(`/customers/${customerId}`);
  redirect(`/customers/${customerId}`);
}

export async function revokeCustomerConsentFormAction(
  customerId: string,
  consentId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");

  if (!isGuid(customerId) || !isGuid(consentId)) {
    throw new Error("Invalid customer consent revoke request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.revokeCustomerConsent(
    customerId,
    consentId,
    { reason: readRequiredString(formData, "reason") },
    options,
  );

  revalidatePath(`/customers/${customerId}`);
  redirect(`/customers/${customerId}`);
}

export async function mergeCustomersFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers", "customers.duplicates.review");

  const masterCustomerId = readRequiredGuid(formData, "masterCustomerId");
  const duplicateCustomerId = readRequiredGuid(formData, "duplicateCustomerId");

  const options = await getCrmApiRequestOptions();
  const mergedCustomerId = await crmApiClient.mergeCustomers(
    {
      masterCustomerId,
      duplicateCustomerId,
      resolvedFields: readResolvedFields(formData),
      reason: readRequiredString(formData, "reason"),
    },
    options,
  );

  revalidatePath("/customers");
  revalidatePath(`/customers/${masterCustomerId}`);
  revalidatePath(`/customers/${duplicateCustomerId}`);
  redirect(`/customers/${mergedCustomerId}`);
}

export async function shareCustomerRecordFormAction(
  customerId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");

  if (!isGuid(customerId)) {
    throw new Error("Invalid customer share request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.shareCustomerRecord(
    customerId,
    {
      sharedWithUserId: readOptionalGuid(formData, "sharedWithUserId"),
      sharedWithTeamId: readOptionalGuid(formData, "sharedWithTeamId"),
      accessLevel: readRequiredInteger(formData, "accessLevel"),
      validUntilUtc: readOptionalString(formData, "validUntilUtc"),
      reason: readRequiredString(formData, "reason"),
    },
    options,
  );

  revalidatePath(`/customers/${customerId}`);
  redirect(`/customers/${customerId}`);
}

export async function createCustomerImportBatchFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers/imports", "canImportCustomer");

  const options = await getCrmApiRequestOptions();
  await crmApiClient.createCustomerImportBatch(
    {
      fileName: readRequiredString(formData, "fileName"),
      source: readRequiredString(formData, "source"),
      rows: readImportRows(formData),
    },
    options,
  );

  revalidatePath("/customers");
  redirect("/customers");
}

export async function previewCustomerImportBatchFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers/imports", "canImportCustomer");

  const options = await getCrmApiRequestOptions();
  await crmApiClient.previewCustomerImportBatch(readRequiredGuid(formData, "batchId"), options);

  revalidatePath("/customers");
  redirect("/customers");
}

export async function validateCustomerImportBatchFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers/imports", "canImportCustomer");

  const options = await getCrmApiRequestOptions();
  await crmApiClient.validateCustomerImportBatch(readRequiredGuid(formData, "batchId"), options);

  revalidatePath("/customers");
  redirect("/customers");
}

export async function commitCustomerImportBatchFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers/imports", "canImportCustomer");

  const options = await getCrmApiRequestOptions();
  const batchId = readRequiredGuid(formData, "batchId");
  await crmApiClient.validateCustomerImportBatch(batchId, options);
  await crmApiClient.commitCustomerImportBatch(
    batchId,
    { duplicateStrategy: readRequiredInteger(formData, "duplicateStrategy") },
    options,
  );

  revalidatePath("/customers");
  redirect("/customers");
}

export async function cancelCustomerImportBatchFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/customers/imports", "canImportCustomer");

  const options = await getCrmApiRequestOptions();
  await crmApiClient.cancelCustomerImportBatch(
    readRequiredGuid(formData, "batchId"),
    { reason: readOptionalString(formData, "reason") },
    options,
  );

  revalidatePath("/customers");
  redirect("/customers");
}
