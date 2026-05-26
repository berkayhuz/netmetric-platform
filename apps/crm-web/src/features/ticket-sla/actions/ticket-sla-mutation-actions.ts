"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import {
  crmApiClient,
  type TicketSlaEscalationRuleUpsertRequest,
  type TicketSlaPolicyUpsertRequest,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { ticketSlaEscalationRuleFormSchema } from "../forms/ticket-sla-escalation-rule-form-schema";
import { ticketSlaPolicyFormSchema } from "../forms/ticket-sla-policy-form-schema";

function revalidateTicketSlaPaths(ticketId?: string | null) {
  revalidatePath("/ticket-sla");
  revalidatePath("/tickets");
  if (ticketId && isGuid(ticketId)) {
    revalidatePath(`/tickets/${ticketId}`);
  }
}

function invalidConfirmationState(): CrmMutationState {
  return { status: "error", message: "Action confirmation is invalid." };
}

function normalizeNotFound(state: CrmMutationState, target: string): CrmMutationState {
  if (state.message === "The requested record no longer exists.") {
    return { status: "error", message: `${target} is already removed or no longer available.` };
  }

  return state;
}

function readOptionalString(formData: FormData, field: string): string | null {
  const value = formData.get(field);
  return typeof value === "string" ? emptyToNull(value) : null;
}

function readRequiredGuid(formData: FormData, field: string): string {
  const value = readOptionalString(formData, field);
  if (!value || !isGuid(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readDateTimeOrNow(formData: FormData, field: string): string {
  const value = readOptionalString(formData, field);
  if (!value) {
    return new Date().toISOString();
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.valueOf())) {
    throw new Error(`Invalid ${field}.`);
  }

  return parsed.toISOString();
}

function toPolicyPayload(formData: FormData): TicketSlaPolicyUpsertRequest | null {
  const parsed = ticketSlaPolicyFormSchema.safeParse({
    policyId: formData.get("policyId"),
    name: formData.get("name"),
    ticketCategoryId: formData.get("ticketCategoryId"),
    priority: formData.get("priority"),
    firstResponseTargetMinutes: formData.get("firstResponseTargetMinutes"),
    resolutionTargetMinutes: formData.get("resolutionTargetMinutes"),
    isDefault: formData.get("isDefault") === "true",
  });

  if (!parsed.success) {
    return null;
  }

  return {
    name: parsed.data.name,
    ticketCategoryId: emptyToNull(parsed.data.ticketCategoryId),
    priority: parsed.data.priority,
    firstResponseTargetMinutes: parsed.data.firstResponseTargetMinutes,
    resolutionTargetMinutes: parsed.data.resolutionTargetMinutes,
    isDefault: parsed.data.isDefault,
  };
}

function toRulePayload(formData: FormData): TicketSlaEscalationRuleUpsertRequest | null {
  const parsed = ticketSlaEscalationRuleFormSchema.safeParse({
    ruleId: formData.get("ruleId"),
    slaPolicyId: formData.get("slaPolicyId"),
    metricType: formData.get("metricType"),
    triggerBeforeOrAfterMinutes: formData.get("triggerBeforeOrAfterMinutes"),
    actionType: formData.get("actionType"),
    targetTeamId: formData.get("targetTeamId"),
    targetUserId: formData.get("targetUserId"),
    isEnabled: formData.get("isEnabled") === "true",
  });

  if (!parsed.success) {
    return null;
  }

  return {
    slaPolicyId: parsed.data.slaPolicyId,
    metricType: parsed.data.metricType,
    triggerBeforeOrAfterMinutes: parsed.data.triggerBeforeOrAfterMinutes,
    actionType: parsed.data.actionType,
    targetTeamId: emptyToNull(parsed.data.targetTeamId),
    targetUserId: emptyToNull(parsed.data.targetUserId),
    isEnabled: parsed.data.isEnabled,
  };
}

export async function createTicketSlaPolicyAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");
  const payload = toPolicyPayload(formData);
  if (!payload) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.createTicketSlaPolicy(payload, options);
    revalidateTicketSlaPaths();
    return { status: "success", message: "SLA policy created." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-sla");
  }
}

export async function updateTicketSlaPolicyAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");
  const policyId = formData.get("policyId");
  if (typeof policyId !== "string" || !isGuid(policyId)) {
    return { status: "error", message: "Invalid policy id." };
  }

  const payload = toPolicyPayload(formData);
  if (!payload) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateTicketSlaPolicy(policyId, payload, options);
    revalidateTicketSlaPaths();
    return { status: "success", message: "SLA policy updated." };
  } catch (error) {
    return normalizeNotFound(mapCrmMutationErrorToState(error, "/ticket-sla"), "SLA policy");
  }
}

export async function deleteTicketSlaPolicyAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");
  if (formData.get("confirm") !== "delete-sla-policy") return invalidConfirmationState();

  const policyId = formData.get("policyId");
  if (typeof policyId !== "string" || !isGuid(policyId)) {
    return { status: "error", message: "Invalid policy id." };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteTicketSlaPolicy(policyId, options);
    revalidateTicketSlaPaths();
    return { status: "success", message: "SLA policy deleted." };
  } catch (error) {
    return normalizeNotFound(mapCrmMutationErrorToState(error, "/ticket-sla"), "SLA policy");
  }
}

export async function createTicketSlaEscalationRuleAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");
  const payload = toRulePayload(formData);
  if (!payload) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.createTicketSlaEscalationRule(payload, options);
    revalidateTicketSlaPaths();
    return { status: "success", message: "Escalation rule created." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-sla");
  }
}

export async function updateTicketSlaEscalationRuleAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");

  const ruleId = formData.get("ruleId");
  if (typeof ruleId !== "string" || !isGuid(ruleId)) {
    return { status: "error", message: "Invalid escalation rule id." };
  }

  const payload = toRulePayload(formData);
  if (!payload) return { status: "error", message: "Please review the highlighted fields." };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateTicketSlaEscalationRule(ruleId, payload, options);
    revalidateTicketSlaPaths();
    return { status: "success", message: "Escalation rule updated." };
  } catch (error) {
    return normalizeNotFound(mapCrmMutationErrorToState(error, "/ticket-sla"), "Escalation rule");
  }
}

export async function attachTicketSlaToTicketAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");

  try {
    const ticketId = readRequiredGuid(formData, "ticketId");
    const options = await getCrmApiRequestOptions();
    await crmApiClient.attachTicketSla(
      {
        ticketId,
        slaPolicyId: readRequiredGuid(formData, "slaPolicyId"),
        createdAtUtc: readDateTimeOrNow(formData, "createdAtUtc"),
      },
      options,
    );

    revalidateTicketSlaPaths(ticketId);
    return { status: "success", message: "SLA policy attached to ticket." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-sla");
  }
}

export async function markTicketFirstResponseAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");

  try {
    const ticketId = readRequiredGuid(formData, "ticketId");
    const options = await getCrmApiRequestOptions();
    await crmApiClient.markTicketFirstResponse(
      {
        ticketId,
        respondedAtUtc: readDateTimeOrNow(formData, "respondedAtUtc"),
      },
      options,
    );

    revalidateTicketSlaPaths(ticketId);
    return { status: "success", message: "First response recorded." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-sla");
  }
}

export async function markTicketResolvedAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");

  try {
    const ticketId = readRequiredGuid(formData, "ticketId");
    const options = await getCrmApiRequestOptions();
    await crmApiClient.markTicketResolved(
      {
        ticketId,
        resolvedAtUtc: readDateTimeOrNow(formData, "resolvedAtUtc"),
      },
      options,
    );

    revalidateTicketSlaPaths(ticketId);
    return { status: "success", message: "Ticket resolution recorded." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-sla");
  }
}

export async function runDueTicketEscalationsAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-sla", "ticketSla.manage");

  try {
    const options = await getCrmApiRequestOptions();
    const result = await crmApiClient.runDueTicketEscalations(
      {
        utcNow: readDateTimeOrNow(formData, "utcNow"),
      },
      options,
    );

    revalidateTicketSlaPaths(readOptionalString(formData, "ticketId"));
    return {
      status: "success",
      message: `Due escalation run processed ${result.processed} ticket(s).`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-sla");
  }
}
