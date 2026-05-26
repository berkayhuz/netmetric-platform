"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type ContactUpsertRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  contactFormSchema,
  type ContactFormInput,
  type ContactFormValues,
} from "../forms/contact-form-schema";

function mapContactPayload(input: ContactFormValues): ContactUpsertRequest {
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
    companyId: emptyToNull(input.companyId),
    customerId: emptyToNull(input.customerId),
    isPrimaryContact: input.isPrimaryContact,
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
      if (first.toLowerCase().includes("linked to a customer or company")) {
        return [[key, [first]] as const];
      }
      const isRequired =
        first.toLowerCase().includes("required") || first.toLowerCase().includes("small");
      const resolved = isRequired
        ? tCrm("crm.contacts.validation.required", locale)
        : tCrm("crm.contacts.validation.invalid", locale);
      return [[key, [resolved]] as const];
    }),
  );
}

export async function createContactAction(input: ContactFormInput): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/contacts/new", "contacts.create");
  const locale = await getRequestLocale();

  const parsed = contactFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createContact(mapContactPayload(parsed.data), options);

    revalidatePath("/contacts");
    revalidatePath(`/contacts/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.contacts.result.created", locale),
      redirectTo: `/contacts/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/contacts/new");
  }
}

export async function updateContactAction(
  contactId: string,
  input: ContactFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/contacts/${contactId}`, "contacts.edit");
  const locale = await getRequestLocale();

  if (!isGuid(contactId)) {
    return {
      status: "error",
      message: tCrm("crm.contacts.validation.invalidId", locale),
    };
  }

  const parsed = contactFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateContact(contactId, mapContactPayload(parsed.data), options);

    revalidatePath("/contacts");
    revalidatePath(`/contacts/${contactId}`);
    revalidatePath(`/contacts/${contactId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.contacts.result.updated", locale),
      redirectTo: `/contacts/${contactId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/contacts/${contactId}/edit`);
  }
}

export async function deleteContactAction(
  contactId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/contacts/${contactId}`, "contacts.delete");
  const locale = await getRequestLocale();

  if (!isGuid(contactId)) {
    return { status: "error", message: tCrm("crm.contacts.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "delete-contact") {
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
    await crmApiClient.deleteContact(contactId, options);

    revalidatePath("/contacts");
    redirect("/contacts");
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, `/contacts/${contactId}`);
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: tCrm("crm.contacts.result.alreadyRemoved", locale),
      };
    }

    return mapped;
  }
}

export async function deleteContactsBulkFromListAction(
  contactIds: string[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/contacts", "contacts.delete");
  const locale = await getRequestLocale();

  const uniqueIds = [...new Set(contactIds)].filter((id) => isGuid(id));
  if (uniqueIds.length === 0) {
    return {
      status: "error",
      message: tCrm("crm.contacts.validation.invalidId", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await Promise.all(uniqueIds.map((id) => crmApiClient.deleteContact(id, options)));
    revalidatePath("/contacts");
    return {
      status: "success",
      message: `${uniqueIds.length} contact(s) deleted.`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/contacts");
  }
}

export async function setPrimaryContactAction(
  contactId: string,
  previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  void previous;
  void formData;
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/contacts/${contactId}`, "contacts.edit");
  const locale = await getRequestLocale();

  if (!isGuid(contactId)) {
    return { status: "error", message: tCrm("crm.contacts.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.setPrimaryContact(contactId, options);

    revalidatePath("/contacts");
    revalidatePath(`/contacts/${contactId}`);
    return { status: "success", message: tCrm("crm.contacts.result.primaryUpdated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/contacts/${contactId}`);
  }
}
