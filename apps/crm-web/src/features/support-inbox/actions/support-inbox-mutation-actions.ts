"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import {
  crmApiClient,
  type SupportInboxConnectionCreateRequest,
  type SupportInboxConnectionUpdateRequest,
  type SupportInboxRuleCreateRequest,
  type SupportInboxRuleUpdateRequest,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

function revalidateSupportInboxPaths() {
  revalidatePath("/support-inbox");
}

function readString(formData: FormData, field: string): string {
  const value = formData.get(field);
  return typeof value === "string" ? value.trim() : "";
}

function readRequiredString(formData: FormData, field: string): string | null {
  const value = readString(formData, field);
  return value.length > 0 ? value : null;
}

function readRequiredGuid(formData: FormData, field: string): string | null {
  const value = readRequiredString(formData, field);
  return value && isGuid(value) ? value : null;
}

function readOptionalGuid(formData: FormData, field: string): string | null {
  const value = emptyToNull(readString(formData, field));
  return value && isGuid(value) ? value : null;
}

function readBoolean(formData: FormData, field: string, fallback = false): boolean {
  const value = formData.get(field);
  if (value === "true") return true;
  if (value === "false") return false;
  return fallback;
}

function readPositiveNumber(formData: FormData, field: string): number | null {
  const parsed = Number.parseInt(readString(formData, field), 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
}

async function validationError(): Promise<CrmMutationState> {
  const locale = await getRequestLocale();
  return { status: "error", message: tCrm("crm.forms.errors.reviewTitle", locale) };
}

function connectionCreatePayload(formData: FormData): SupportInboxConnectionCreateRequest | null {
  const name = readRequiredString(formData, "name");
  const emailAddress = readRequiredString(formData, "emailAddress");
  const host = readRequiredString(formData, "host");
  const port = readPositiveNumber(formData, "port");
  const username = readRequiredString(formData, "username");
  const secretReference = readRequiredString(formData, "secretReference");
  const provider = readPositiveNumber(formData, "provider");

  if (!name || !emailAddress || !host || !port || !username || !secretReference || !provider) {
    return null;
  }

  return {
    name,
    provider,
    emailAddress,
    host,
    port,
    username,
    secretReference,
    useSsl: readBoolean(formData, "useSsl", true),
  };
}

function connectionUpdatePayload(formData: FormData): SupportInboxConnectionUpdateRequest | null {
  const name = readRequiredString(formData, "name");
  const host = readRequiredString(formData, "host");
  const port = readPositiveNumber(formData, "port");
  const username = readRequiredString(formData, "username");
  const secretReference = readRequiredString(formData, "secretReference");

  if (!name || !host || !port || !username || !secretReference) {
    return null;
  }

  return {
    name,
    host,
    port,
    username,
    secretReference,
    useSsl: readBoolean(formData, "useSsl", true),
    isActive: readBoolean(formData, "isActive", true),
  };
}

function ruleCreatePayload(formData: FormData): SupportInboxRuleCreateRequest | null {
  const connectionId = readRequiredGuid(formData, "connectionId");
  const name = readRequiredString(formData, "name");

  if (!connectionId || !name) {
    return null;
  }

  return {
    connectionId,
    name,
    matchSender: emptyToNull(readString(formData, "matchSender")),
    matchSubjectContains: emptyToNull(readString(formData, "matchSubjectContains")),
    assignToQueueId: readOptionalGuid(formData, "assignToQueueId"),
    ticketCategoryId: readOptionalGuid(formData, "ticketCategoryId"),
    slaPolicyId: readOptionalGuid(formData, "slaPolicyId"),
    autoCreateTicket: readBoolean(formData, "autoCreateTicket", true),
  };
}

function ruleUpdatePayload(formData: FormData): SupportInboxRuleUpdateRequest | null {
  const name = readRequiredString(formData, "name");

  if (!name) {
    return null;
  }

  return {
    name,
    matchSender: emptyToNull(readString(formData, "matchSender")),
    matchSubjectContains: emptyToNull(readString(formData, "matchSubjectContains")),
    assignToQueueId: readOptionalGuid(formData, "assignToQueueId"),
    ticketCategoryId: readOptionalGuid(formData, "ticketCategoryId"),
    slaPolicyId: readOptionalGuid(formData, "slaPolicyId"),
    autoCreateTicket: readBoolean(formData, "autoCreateTicket", true),
    isActive: readBoolean(formData, "isActive", true),
  };
}

export async function createSupportInboxConnectionAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/support-inbox", "supportInbox.manage");
  const locale = await getRequestLocale();
  const payload = connectionCreatePayload(formData);
  if (!payload) return validationError();

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.createSupportInboxConnection(payload, options);
    revalidateSupportInboxPaths();
    return {
      status: "success",
      message: tCrm("crm.supportInbox.result.connectionCreated", locale),
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/support-inbox");
  }
}

export async function updateSupportInboxConnectionAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/support-inbox", "supportInbox.manage");
  const locale = await getRequestLocale();
  const connectionId = readRequiredGuid(formData, "connectionId");
  const payload = connectionUpdatePayload(formData);
  if (!connectionId || !payload) return validationError();

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateSupportInboxConnection(connectionId, payload, options);
    revalidateSupportInboxPaths();
    return {
      status: "success",
      message: tCrm("crm.supportInbox.result.connectionUpdated", locale),
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/support-inbox");
  }
}

export async function triggerSupportInboxSyncAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/support-inbox", "supportInbox.manage");
  const locale = await getRequestLocale();
  const connectionId = readRequiredGuid(formData, "connectionId");
  if (!connectionId) return validationError();

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.triggerSupportInboxSync(
      connectionId,
      { dryRun: readBoolean(formData, "dryRun") },
      options,
    );
    revalidateSupportInboxPaths();
    return { status: "success", message: tCrm("crm.supportInbox.result.syncQueued", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/support-inbox");
  }
}

export async function createSupportInboxRuleAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/support-inbox", "supportInbox.manage");
  const locale = await getRequestLocale();
  const payload = ruleCreatePayload(formData);
  if (!payload) return validationError();

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.createSupportInboxRule(payload, options);
    revalidateSupportInboxPaths();
    return { status: "success", message: tCrm("crm.supportInbox.result.ruleCreated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/support-inbox");
  }
}

export async function updateSupportInboxRuleAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/support-inbox", "supportInbox.manage");
  const locale = await getRequestLocale();
  const ruleId = readRequiredGuid(formData, "ruleId");
  const payload = ruleUpdatePayload(formData);
  if (!ruleId || !payload) return validationError();

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateSupportInboxRule(ruleId, payload, options);
    revalidateSupportInboxPaths();
    return { status: "success", message: tCrm("crm.supportInbox.result.ruleUpdated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/support-inbox");
  }
}
