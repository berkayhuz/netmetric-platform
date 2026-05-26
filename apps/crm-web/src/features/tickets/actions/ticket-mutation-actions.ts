"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type TicketUpdateRequest, type TicketUpsertRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  ticketFormSchema,
  type TicketFormInput,
  type TicketFormValues,
} from "../forms/ticket-form-schema";

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

function toApiDateTime(value?: string): string | null {
  if (!value) {
    return null;
  }

  const normalized = value.trim();
  if (!normalized) {
    return null;
  }

  const parsed = new Date(normalized);
  if (Number.isNaN(parsed.valueOf())) {
    return null;
  }

  return parsed.toISOString();
}

function mapTicketPayload(input: TicketFormValues): TicketUpsertRequest {
  return {
    subject: input.subject.trim(),
    description: emptyToNull(input.description),
    ticketType: input.ticketType,
    channel: input.channel,
    priority: input.priority,
    assignedUserId: emptyToNull(input.assignedUserId),
    customerId: emptyToNull(input.customerId),
    contactId: emptyToNull(input.contactId),
    ticketCategoryId: emptyToNull(input.ticketCategoryId),
    slaPolicyId: emptyToNull(input.slaPolicyId),
    firstResponseDueAt: toApiDateTime(input.firstResponseDueAt),
    resolveDueAt: toApiDateTime(input.resolveDueAt),
    notes: emptyToNull(input.notes),
  };
}

function mapTicketUpdatePayload(input: TicketFormValues): TicketUpdateRequest {
  return {
    ...mapTicketPayload(input),
    rowVersion: emptyToNull(input.rowVersion),
  };
}

export async function createTicketAction(input: TicketFormInput): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/tickets/new", "tickets.create");
  const locale = await getRequestLocale();

  const parsed = ticketFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createTicket(mapTicketPayload(parsed.data), options);

    revalidatePath("/tickets");
    revalidatePath(`/tickets/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.tickets.result.created", locale),
      redirectTo: `/tickets/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/tickets/new");
  }
}

export async function updateTicketAction(
  ticketId: string,
  input: TicketFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tickets/${ticketId}`, "tickets.edit");
  const locale = await getRequestLocale();

  if (!isGuid(ticketId)) {
    return {
      status: "error",
      message: tCrm("crm.tickets.validation.invalidId", locale),
    };
  }

  const parsed = ticketFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateTicket(ticketId, mapTicketUpdatePayload(parsed.data), options);

    revalidatePath("/tickets");
    revalidatePath(`/tickets/${ticketId}`);
    revalidatePath(`/tickets/${ticketId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.tickets.result.updated", locale),
      redirectTo: `/tickets/${ticketId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/tickets/${ticketId}/edit`);
  }
}

export async function deleteTicketAction(
  ticketId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tickets/${ticketId}`, "tickets.delete");
  const locale = await getRequestLocale();

  if (!isGuid(ticketId)) {
    return { status: "error", message: tCrm("crm.tickets.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "delete-ticket") {
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
    await crmApiClient.deleteTicket(ticketId, options);

    revalidatePath("/tickets");
    revalidatePath(`/tickets/${ticketId}`);
    redirect("/tickets");
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, `/tickets/${ticketId}`);
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: tCrm("crm.tickets.result.alreadyRemoved", locale),
      };
    }

    return mapped;
  }
}

export async function deleteTicketsBulkFromListAction(
  ticketIds: string[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/tickets", "tickets.delete");
  const locale = await getRequestLocale();

  const uniqueIds = [...new Set(ticketIds)].filter((id) => isGuid(id));
  if (uniqueIds.length === 0) {
    return {
      status: "error",
      message: tCrm("crm.tickets.validation.invalidId", locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await Promise.all(uniqueIds.map((id) => crmApiClient.deleteTicket(id, options)));
    revalidatePath("/tickets");
    return {
      status: "success",
      message: `${uniqueIds.length} ticket(s) deleted.`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/tickets");
  }
}
