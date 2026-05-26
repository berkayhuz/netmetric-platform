"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type CompanyUpsertRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  companyFormSchema,
  type CompanyFormInput,
  type CompanyFormValues,
} from "../forms/company-form-schema";

function mapCompanyPayload(input: CompanyFormValues): CompanyUpsertRequest {
  return {
    name: input.name.trim(),
    taxNumber: emptyToNull(input.taxNumber),
    taxOffice: emptyToNull(input.taxOffice),
    website: emptyToNull(input.website),
    email: emptyToNull(input.email),
    phone: emptyToNull(input.phone),
    sector: emptyToNull(input.sector),
    employeeCountRange: emptyToNull(input.employeeCountRange),
    annualRevenue: input.annualRevenue ?? null,
    description: emptyToNull(input.description),
    notes: emptyToNull(input.notes),
    companyType: input.companyType,
    ownerUserId: emptyToNull(input.ownerUserId),
    parentCompanyId: emptyToNull(input.parentCompanyId),
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
        ? tCrm("crm.companies.validation.required", locale)
        : tCrm("crm.companies.validation.invalid", locale);
      return [[key, [resolved]] as const];
    }),
  );
}

function getLogoUploadPayload(formData: FormData, locale: string): FormData | CrmMutationState {
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
  payload.set("file", image, image.name ?? "company-logo");
  return payload;
}

export async function createCompanyAction(input: CompanyFormInput): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/companies/new", "companies.create");
  const locale = await getRequestLocale();

  const parsed = companyFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createCompany(mapCompanyPayload(parsed.data), options);

    revalidatePath("/companies");
    revalidatePath(`/companies/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.companies.result.created", locale),
      redirectTo: `/companies/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/companies/new");
  }
}

export async function updateCompanyAction(
  companyId: string,
  input: CompanyFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/companies/${companyId}`, "companies.edit");
  const locale = await getRequestLocale();

  if (!isGuid(companyId)) {
    return {
      status: "error",
      message: tCrm("crm.companies.validation.invalidId", locale),
    };
  }

  const parsed = companyFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateCompany(companyId, mapCompanyPayload(parsed.data), options);

    revalidatePath("/companies");
    revalidatePath(`/companies/${companyId}`);
    revalidatePath(`/companies/${companyId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.companies.result.updated", locale),
      redirectTo: `/companies/${companyId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/companies/${companyId}/edit`);
  }
}

export async function deleteCompanyAction(
  companyId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/companies/${companyId}`, "companies.delete");
  const locale = await getRequestLocale();

  if (!isGuid(companyId)) {
    return { status: "error", message: tCrm("crm.companies.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "delete-company") {
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
    await crmApiClient.deleteCompany(companyId, options);

    revalidatePath("/companies");
    redirect("/companies");
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, `/companies/${companyId}`);
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: tCrm("crm.companies.result.alreadyRemoved", locale),
      };
    }

    return mapped;
  }
}

export async function deleteCompaniesBulkFromListAction(
  companyIds: string[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/companies", "companies.delete");
  const locale = await getRequestLocale();

  const uniqueIds = [...new Set(companyIds)].filter((id) => isGuid(id));
  if (uniqueIds.length === 0) {
    return {
      status: "error",
      message: tCrm("crm.companies.validation.invalidId", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await Promise.all(uniqueIds.map((id) => crmApiClient.deleteCompany(id, options)));
    revalidatePath("/companies");
    return {
      status: "success",
      message: `${uniqueIds.length} company(ies) deleted.`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/companies");
  }
}

export async function uploadCompanyLogoAction(
  companyId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/companies/${companyId}`, "companies.edit");
  const locale = await getRequestLocale();

  if (!isGuid(companyId)) {
    return { status: "error", message: tCrm("crm.companies.validation.invalidId", locale) };
  }

  const payload = getLogoUploadPayload(formData, locale);
  if (!(payload instanceof FormData)) {
    return payload;
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.uploadCompanyLogo(companyId, payload, options);

    revalidatePath("/companies");
    revalidatePath(`/companies/${companyId}`);
    return { status: "success", message: tCrm("crm.companies.result.logoUpdated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/companies/${companyId}`);
  }
}

export async function removeCompanyLogoAction(
  companyId: string,
  _previous: CrmMutationState,
  _formData: FormData,
): Promise<CrmMutationState> {
  void _previous;
  void _formData;

  await assertSameOriginRequest();
  await requireCrmActionCapability(`/companies/${companyId}`, "companies.edit");
  const locale = await getRequestLocale();

  if (!isGuid(companyId)) {
    return { status: "error", message: tCrm("crm.companies.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.removeCompanyLogo(companyId, options);

    revalidatePath("/companies");
    revalidatePath(`/companies/${companyId}`);
    return { status: "success", message: tCrm("crm.companies.result.logoRemoved", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/companies/${companyId}`);
  }
}

export async function activateCompanyAction(
  companyId: string,
  previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  void previous;
  void formData;
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/companies/${companyId}`, "companies.edit");
  const locale = await getRequestLocale();

  if (!isGuid(companyId)) {
    return { status: "error", message: tCrm("crm.companies.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.activateCompany(companyId, options);

    revalidatePath("/companies");
    revalidatePath(`/companies/${companyId}`);
    return { status: "success", message: tCrm("crm.companies.result.activated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/companies/${companyId}`);
  }
}

export async function deactivateCompanyAction(
  companyId: string,
  previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  void previous;
  void formData;
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/companies/${companyId}`, "companies.edit");
  const locale = await getRequestLocale();

  if (!isGuid(companyId)) {
    return { status: "error", message: tCrm("crm.companies.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deactivateCompany(companyId, options);

    revalidatePath("/companies");
    revalidatePath(`/companies/${companyId}`);
    return { status: "success", message: tCrm("crm.companies.result.deactivated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/companies/${companyId}`);
  }
}
