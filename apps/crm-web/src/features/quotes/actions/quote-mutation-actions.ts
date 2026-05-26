"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import {
  CrmApiError,
  crmApiClient,
  type QuoteLineUpsertRequest,
  type QuoteUpdateRequest,
  type QuoteUpsertRequest,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  quoteFormSchema,
  type QuoteFormInput,
  type QuoteFormValues,
} from "../forms/quote-form-schema";

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

function mapQuoteLines(input: QuoteFormValues): QuoteLineUpsertRequest[] {
  return input.items.map((item) => ({
    productId: item.productId,
    description: emptyToNull(item.description),
    quantity: item.quantity,
    unitPrice: Number(item.unitPrice),
    discountRate: item.discountRate,
    taxRate: item.taxRate,
  }));
}

function mapQuotePayload(input: QuoteFormValues): QuoteUpsertRequest {
  return {
    quoteNumber: input.quoteNumber.trim(),
    proposalTitle: emptyToNull(input.proposalTitle),
    proposalSummary: emptyToNull(input.proposalSummary),
    proposalBody: emptyToNull(input.proposalBody),
    quoteDate: input.quoteDate,
    validUntil: emptyToNull(input.validUntil),
    opportunityId: emptyToNull(input.opportunityId),
    customerId: emptyToNull(input.customerId),
    ownerUserId: emptyToNull(input.ownerUserId),
    currencyCode: input.currencyCode.trim().toUpperCase(),
    exchangeRate: Number(input.exchangeRate),
    termsAndConditions: emptyToNull(input.termsAndConditions),
    proposalTemplateId: emptyToNull(input.proposalTemplateId),
    items: mapQuoteLines(input),
  };
}

function mapQuoteUpdatePayload(input: QuoteFormValues): QuoteUpdateRequest {
  const rowVersion = input.rowVersion?.trim();

  return {
    ...mapQuotePayload(input),
    rowVersion: rowVersion && rowVersion.length > 0 ? rowVersion : "",
  };
}

export async function createQuoteAction(input: QuoteFormInput): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/quotes/new", "quotes.create");

  const parsed = quoteFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: "Please review the highlighted fields.",
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createQuote(mapQuotePayload(parsed.data), options);

    revalidatePath("/quotes");
    revalidatePath(`/quotes/${created.id}`);

    return {
      status: "success",
      message: "Quote created successfully.",
      redirectTo: `/quotes/${created.id}`,
    };
  } catch (error) {
    const submittedProposalTemplateId = parsed.data.proposalTemplateId?.trim();
    if (
      error instanceof CrmApiError &&
      submittedProposalTemplateId &&
      (error.kind === "server_error" || error.kind === "upstream_unavailable")
    ) {
      return {
        status: "error",
        message:
          "Selected proposal template is unavailable. Choose another template or leave it empty.",
        fieldErrors: {
          proposalTemplateId: [
            "Selected proposal template is unavailable. Choose another template or leave it empty.",
          ],
        },
      };
    }

    return mapCrmMutationErrorToState(error, "/quotes/new");
  }
}

export async function updateQuoteAction(
  quoteId: string,
  input: QuoteFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");

  if (!isGuid(quoteId)) {
    return {
      status: "error",
      message: "Invalid quote id.",
    };
  }

  const parsed = quoteFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: "Please review the highlighted fields.",
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateQuote(quoteId, mapQuoteUpdatePayload(parsed.data), options);

    revalidatePath("/quotes");
    revalidatePath(`/quotes/${quoteId}`);
    revalidatePath(`/quotes/${quoteId}/edit`);

    return {
      status: "success",
      message: "Quote updated successfully.",
      redirectTo: `/quotes/${quoteId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/quotes/${quoteId}/edit`);
  }
}

export async function deleteQuoteAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.delete");

  if (!isGuid(quoteId)) {
    return { status: "error", message: "Invalid quote id." };
  }

  if (formData.get("confirm") !== "delete-quote") {
    return { status: "error", message: "Delete confirmation is invalid." };
  }

  const confirmText = formData.get("confirmText");
  if (typeof confirmText !== "string" || confirmText.trim().length === 0) {
    return {
      status: "error",
      message: "Please type the record name to confirm deletion.",
      fieldErrors: { confirmText: ["Confirmation text is required."] },
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteQuote(quoteId, options);

    revalidatePath("/quotes");
    redirect("/quotes");
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, `/quotes/${quoteId}`);
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: "Quote is already removed or no longer available.",
      };
    }

    return mapped;
  }
}

export async function deleteQuotesBulkFromListAction(
  quoteIds: string[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/quotes", "quotes.delete");

  const uniqueIds = [...new Set(quoteIds)].filter((id) => isGuid(id));
  if (uniqueIds.length === 0) {
    return { status: "error", message: "Invalid quote id." };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await Promise.all(uniqueIds.map((id) => crmApiClient.deleteQuote(id, options)));
    revalidatePath("/quotes");
    return {
      status: "success",
      message: `${uniqueIds.length} quote(s) deleted.`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/quotes");
  }
}
