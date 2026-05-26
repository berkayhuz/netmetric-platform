"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type AddressUpsertRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  addressFormSchema,
  type AddressFormInput,
  type AddressFormValues,
} from "../forms/address-form-schema";

function mapAddressPayload(input: AddressFormValues): AddressUpsertRequest {
  return {
    addressType: input.addressType,
    line1: input.line1.trim(),
    line2: emptyToNull(input.line2),
    district: emptyToNull(input.district),
    city: emptyToNull(input.city),
    state: emptyToNull(input.state),
    country: emptyToNull(input.country),
    zipCode: emptyToNull(input.zipCode),
    isDefault: input.isDefault,
    rowVersion: emptyToNull(input.rowVersion),
  };
}

function mapZodErrors(fieldErrors: Record<string, string[] | undefined>): Record<string, string[]> {
  return Object.fromEntries(
    Object.entries(fieldErrors).flatMap(([key, errors]) => {
      if (!errors || errors.length === 0) {
        return [];
      }

      return [[key, errors] as const];
    }),
  );
}

function revalidateAddressOwnerPaths(entityType: "customer" | "company", entityId: string): void {
  if (entityType === "customer") {
    revalidatePath("/customers");
    revalidatePath(`/customers/${entityId}`);
    revalidatePath(`/customers/${entityId}/edit`);
    return;
  }

  revalidatePath("/companies");
  revalidatePath(`/companies/${entityId}`);
  revalidatePath(`/companies/${entityId}/edit`);
}

function resolveReturnPath(entityType: "customer" | "company", entityId: string): string {
  return entityType === "customer" ? `/customers/${entityId}` : `/companies/${entityId}`;
}

function resolveAddressEditCapability(entityType: "customer" | "company") {
  return entityType === "customer" ? "customers.edit" : "companies.edit";
}

export async function createCustomerAddressAction(
  customerId: string,
  input: AddressFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/customers/${customerId}`, "customers.edit");

  if (!isGuid(customerId)) {
    return { status: "error", message: "Invalid customer id." };
  }

  const parsed = addressFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: "Please review the highlighted fields.",
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.addAddressToCustomer(customerId, mapAddressPayload(parsed.data), options);
    revalidateAddressOwnerPaths("customer", customerId);
    return { status: "success", message: "Address added successfully." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/customers/${customerId}`);
  }
}

export async function createCompanyAddressAction(
  companyId: string,
  input: AddressFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/companies/${companyId}`, "companies.edit");

  if (!isGuid(companyId)) {
    return { status: "error", message: "Invalid company id." };
  }

  const parsed = addressFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: "Please review the highlighted fields.",
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.addAddressToCompany(companyId, mapAddressPayload(parsed.data), options);
    revalidateAddressOwnerPaths("company", companyId);
    return { status: "success", message: "Address added successfully." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/companies/${companyId}`);
  }
}

export async function updateAddressAction(
  entityType: "customer" | "company",
  entityId: string,
  addressId: string,
  input: AddressFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(
    resolveReturnPath(entityType, entityId),
    resolveAddressEditCapability(entityType),
  );

  if (!isGuid(entityId)) {
    return { status: "error", message: `Invalid ${entityType} id.` };
  }

  if (!isGuid(addressId)) {
    return { status: "error", message: "Invalid address id." };
  }

  const parsed = addressFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: "Please review the highlighted fields.",
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateAddress(addressId, mapAddressPayload(parsed.data), options);
    revalidateAddressOwnerPaths(entityType, entityId);
    return { status: "success", message: "Address updated successfully." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, resolveReturnPath(entityType, entityId));
  }
}

export async function deleteAddressAction(
  entityType: "customer" | "company",
  entityId: string,
  addressId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(
    resolveReturnPath(entityType, entityId),
    resolveAddressEditCapability(entityType),
  );

  if (!isGuid(entityId)) {
    return { status: "error", message: `Invalid ${entityType} id.` };
  }

  if (!isGuid(addressId)) {
    return { status: "error", message: "Invalid address id." };
  }

  if (formData.get("confirm") !== "delete-address") {
    return { status: "error", message: "Delete confirmation is invalid." };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteAddress(addressId, options);
    revalidateAddressOwnerPaths(entityType, entityId);
    return { status: "success", message: "Address deleted successfully." };
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, resolveReturnPath(entityType, entityId));
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: "Address is already removed or no longer available.",
      };
    }

    return mapped;
  }
}

export async function setDefaultAddressAction(
  entityType: "customer" | "company",
  entityId: string,
  addressId: string,
  formData: FormData,
): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(
    resolveReturnPath(entityType, entityId),
    resolveAddressEditCapability(entityType),
  );

  if (!isGuid(entityId) || !isGuid(addressId)) {
    throw new Error("Invalid address default request.");
  }

  if (formData.get("confirm") !== "set-default-address") {
    throw new Error("Invalid address default confirmation.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.setDefaultAddress(addressId, options);
  revalidateAddressOwnerPaths(entityType, entityId);
}
