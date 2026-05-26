"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import {
  crmApiClient,
  type OpportunityUpdateRequest,
  type OpportunityUpsertRequest,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  opportunityFormSchema,
  type OpportunityFormInput,
  type OpportunityFormValues,
} from "../forms/opportunity-form-schema";

function mapOpportunityPayload(input: OpportunityFormValues): OpportunityUpsertRequest {
  const estimatedAmount = Number(input.estimatedAmount);
  const expectedRevenue = input.expectedRevenue ? Number(input.expectedRevenue) : null;

  return {
    opportunityCode: input.opportunityCode.trim(),
    name: input.name.trim(),
    description: emptyToNull(input.description),
    estimatedAmount,
    expectedRevenue:
      expectedRevenue === null || Number.isNaN(expectedRevenue) ? null : expectedRevenue,
    probability: input.probability,
    estimatedCloseDate: emptyToNull(input.estimatedCloseDate),
    stage: input.stage,
    status: input.status,
    priority: input.priority,
    // OpportunityManagement write model no longer accepts direct LeadId links.
    leadId: null,
    customerId: null,
    ownerUserId: emptyToNull(input.ownerUserId),
    notes: emptyToNull(input.notes),
  };
}

function mapOpportunityUpdatePayload(input: OpportunityFormValues): OpportunityUpdateRequest {
  const rowVersion = input.rowVersion?.trim();

  return {
    ...mapOpportunityPayload(input),
    rowVersion: rowVersion && rowVersion.length > 0 ? rowVersion : "",
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

      return [[key, [tCrm("crm.opportunities.validation.invalid", locale)]] as const];
    }),
  );
}

export async function createOpportunityAction(
  input: OpportunityFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/opportunities/new", "opportunities.create");
  const locale = await getRequestLocale();

  const parsed = opportunityFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createOpportunity(
      mapOpportunityPayload(parsed.data),
      options,
    );

    revalidatePath("/opportunities");
    revalidatePath("/pipeline");
    revalidatePath(`/opportunities/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.opportunities.result.created", locale),
      redirectTo: `/opportunities/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/opportunities/new");
  }
}

export async function updateOpportunityAction(
  opportunityId: string,
  input: OpportunityFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.edit");
  const locale = await getRequestLocale();

  if (!isGuid(opportunityId)) {
    return {
      status: "error",
      message: tCrm("crm.opportunities.validation.invalidId", locale),
    };
  }

  const parsed = opportunityFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateOpportunity(
      opportunityId,
      mapOpportunityUpdatePayload(parsed.data),
      options,
    );

    revalidatePath("/opportunities");
    revalidatePath("/pipeline");
    revalidatePath(`/opportunities/${opportunityId}`);
    revalidatePath(`/opportunities/${opportunityId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.opportunities.result.updated", locale),
      redirectTo: `/opportunities/${opportunityId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/opportunities/${opportunityId}/edit`);
  }
}

export async function deleteOpportunityAction(
  opportunityId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/opportunities/${opportunityId}`, "opportunities.delete");
  const locale = await getRequestLocale();

  if (!isGuid(opportunityId)) {
    return { status: "error", message: tCrm("crm.opportunities.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "delete-opportunity") {
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
    await crmApiClient.deleteOpportunity(opportunityId, options);

    revalidatePath("/opportunities");
    revalidatePath("/pipeline");
    redirect("/opportunities");
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, `/opportunities/${opportunityId}`);
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: tCrm("crm.opportunities.result.alreadyRemoved", locale),
      };
    }

    return mapped;
  }
}

export async function deleteOpportunitiesBulkFromListAction(
  opportunityIds: string[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/opportunities", "opportunities.delete");
  const locale = await getRequestLocale();

  const uniqueIds = [...new Set(opportunityIds)].filter((id) => isGuid(id));
  if (uniqueIds.length === 0) {
    return {
      status: "error",
      message: tCrm("crm.opportunities.validation.invalidId", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await Promise.all(uniqueIds.map((id) => crmApiClient.deleteOpportunity(id, options)));
    revalidatePath("/opportunities");
    revalidatePath("/pipeline");
    return {
      status: "success",
      message: `${uniqueIds.length} opportunit(y/ies) deleted.`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/opportunities");
  }
}
