"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

const restoreCapabilities = {
  contact: "contacts.delete",
  customer: "customers.delete",
  company: "companies.delete",
  lead: "leads.delete",
  deal: "deals.delete",
  opportunity: "opportunities.delete",
  quote: "quotes.delete",
  ticket: "tickets.delete",
  productcatalogitem: "productCatalog.manage",
} as const;

type RestorableTrashEntityType = keyof typeof restoreCapabilities;

function isRestorableTrashEntityType(value: string): value is RestorableTrashEntityType {
  return (
    value === "contact" ||
    value === "customer" ||
    value === "company" ||
    value === "lead" ||
    value === "deal" ||
    value === "opportunity" ||
    value === "quote" ||
    value === "ticket" ||
    value === "productcatalogitem"
  );
}

export async function restoreTrashItemAction(
  trashItemId: string,
  entityType: string,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  const normalizedType = entityType.trim().toLowerCase();
  if (!isRestorableTrashEntityType(normalizedType)) {
    return {
      status: "error",
      message: "Unsupported trash item type.",
    };
  }

  await requireCrmActionCapability("/trash", restoreCapabilities[normalizedType]);
  await getRequestLocale();

  if (!isGuid(trashItemId)) {
    return {
      status: "error",
      message: "Invalid trash item id.",
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.restoreTrashItem(trashItemId, options);
    revalidatePath("/trash");
    revalidatePath("/contacts");
    revalidatePath("/customers");
    revalidatePath("/companies");
    revalidatePath("/leads");
    revalidatePath("/deals");
    revalidatePath("/opportunities");
    revalidatePath("/quotes");
    revalidatePath("/tickets");
    revalidatePath("/product-catalog");

    return {
      status: "success",
      message: "Item restored.",
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/trash");
  }
}
