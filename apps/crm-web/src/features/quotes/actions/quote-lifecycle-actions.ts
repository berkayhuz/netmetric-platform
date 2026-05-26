"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  quoteCreateRevisionSchema,
  quoteLifecycleDateNoteSchema,
  quoteLifecycleNoteSchema,
  quoteLifecycleReasonSchema,
} from "../forms/quote-lifecycle-action-schema";

function revalidateQuoteRoutes(quoteId: string) {
  revalidatePath("/quotes");
  revalidatePath(`/quotes/${quoteId}`);
  revalidatePath(`/quotes/${quoteId}/edit`);
}

function invalidIdState(): CrmMutationState {
  return { status: "error", message: "Invalid quote id." };
}

function invalidConfirmationState(): CrmMutationState {
  return { status: "error", message: "Action confirmation is invalid." };
}

function normalizeNotFoundState(state: CrmMutationState): CrmMutationState {
  if (state.message === "The requested record no longer exists.") {
    return {
      status: "error",
      message: "Quote is already removed or no longer available.",
    };
  }

  return state;
}

export async function submitQuoteAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "submit-quote") return invalidConfirmationState();

  const parsed = quoteLifecycleNoteSchema.safeParse({
    note: formData.get("note"),
    rowVersion: formData.get("rowVersion"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.submitQuote(
      quoteId,
      { note: emptyToNull(parsed.data.note), rowVersion: emptyToNull(parsed.data.rowVersion) },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    return { status: "success", message: "Quote submitted for approval." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}

export async function approveQuoteAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "approve-quote") return invalidConfirmationState();

  const parsed = quoteLifecycleNoteSchema.safeParse({
    note: formData.get("note"),
    rowVersion: formData.get("rowVersion"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.approveQuote(
      quoteId,
      { note: emptyToNull(parsed.data.note), rowVersion: emptyToNull(parsed.data.rowVersion) },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    return { status: "success", message: "Quote approved." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}

export async function rejectQuoteAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "reject-quote") return invalidConfirmationState();

  const parsed = quoteLifecycleReasonSchema.safeParse({
    reason: formData.get("reason"),
    rowVersion: formData.get("rowVersion"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.rejectQuote(
      quoteId,
      { reason: parsed.data.reason, rowVersion: emptyToNull(parsed.data.rowVersion) },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    return { status: "success", message: "Quote rejected." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}

export async function markQuoteSentAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "send-quote") return invalidConfirmationState();

  const parsed = quoteLifecycleDateNoteSchema.safeParse({
    at: formData.get("at"),
    note: formData.get("note"),
    rowVersion: formData.get("rowVersion"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.markQuoteSent(
      quoteId,
      {
        at: emptyToNull(parsed.data.at),
        note: emptyToNull(parsed.data.note),
        rowVersion: emptyToNull(parsed.data.rowVersion),
      },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    return { status: "success", message: "Quote marked as sent." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}

export async function acceptQuoteAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "accept-quote") return invalidConfirmationState();

  const parsed = quoteLifecycleDateNoteSchema.safeParse({
    at: formData.get("at"),
    note: formData.get("note"),
    rowVersion: formData.get("rowVersion"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.acceptQuote(
      quoteId,
      {
        at: emptyToNull(parsed.data.at),
        note: emptyToNull(parsed.data.note),
        rowVersion: emptyToNull(parsed.data.rowVersion),
      },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    return { status: "success", message: "Quote accepted." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}

export async function declineQuoteAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "decline-quote") return invalidConfirmationState();

  const parsed = quoteLifecycleReasonSchema.safeParse({
    reason: formData.get("reason"),
    rowVersion: formData.get("rowVersion"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.declineQuote(
      quoteId,
      {
        at: null,
        reason: parsed.data.reason,
        rowVersion: emptyToNull(parsed.data.rowVersion),
      },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    return { status: "success", message: "Quote declined." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}

export async function expireQuoteAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "expire-quote") return invalidConfirmationState();

  const parsed = quoteLifecycleDateNoteSchema.safeParse({
    at: formData.get("at"),
    note: formData.get("note"),
    rowVersion: formData.get("rowVersion"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.expireQuote(
      quoteId,
      {
        at: emptyToNull(parsed.data.at),
        note: emptyToNull(parsed.data.note),
        rowVersion: emptyToNull(parsed.data.rowVersion),
      },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    return { status: "success", message: "Quote expired." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}

export async function createQuoteRevisionAction(
  quoteId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/quotes/${quoteId}`, "quotes.edit");
  if (!isGuid(quoteId)) return invalidIdState();
  if (formData.get("confirm") !== "revise-quote") return invalidConfirmationState();

  const parsed = quoteCreateRevisionSchema.safeParse({
    newQuoteNumber: formData.get("newQuoteNumber"),
  });
  if (!parsed.success) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    const revised = await crmApiClient.createQuoteRevision(
      quoteId,
      { newQuoteNumber: parsed.data.newQuoteNumber },
      options,
    );
    revalidateQuoteRoutes(quoteId);
    revalidatePath(`/quotes/${revised.id}`);
    return { status: "success", message: "Quote revision created successfully." };
  } catch (error) {
    return normalizeNotFoundState(mapCrmMutationErrorToState(error, `/quotes/${quoteId}`));
  }
}
