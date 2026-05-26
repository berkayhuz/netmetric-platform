"use server";

import { revalidatePath } from "next/cache";
import { createServerPerformanceLogger } from "@netmetric/observability/server";
import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";

import { isGuid } from "@/features/shared/data/guid";
import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

const customerIntelligenceMutationPerf = createServerPerformanceLogger({
  app: "crm-web",
  component: "customer-intelligence-mutations",
  enabled: process.env.NETMETRIC_PERF_LOG === "1",
});

function readOptionalString(formData: FormData, field: string): string | null {
  const value = formData.get(field);
  if (typeof value !== "string") {
    return null;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

function readRequiredString(formData: FormData, field: string): string {
  const value = readOptionalString(formData, field);
  if (!value) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readRequiredGuid(formData: FormData, field: string): string {
  const value = readRequiredString(formData, field);
  if (!isGuid(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readOptionalDecimal(formData: FormData, field: string): number | null {
  const value = readOptionalString(formData, field);
  if (!value) {
    return null;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function completeAction(customerId: string): CrmMutationState {
  revalidatePath("/customer-intelligence");
  revalidatePath(`/customer-intelligence?customerId=${customerId}`);
  return {
    status: "success",
    message: "Operation completed.",
    redirectTo: `/customer-intelligence?customerId=${customerId}`,
  };
}

function denyAction(locale: string): CrmMutationState {
  return {
    status: "error",
    message: tCrm("crm.accessDenied.title", locale),
  };
}

function invalidField(locale: string, field: string): CrmMutationState {
  return {
    status: "error",
    message: tCrm("crm.forms.result.tryAgain", locale),
    fieldErrors: { [field]: [tCrm("crm.forms.errors.reviewTitle", locale)] },
  };
}

export async function detectDuplicatesFormAction(
  _previous: CrmMutationState = initialCrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  const session = await requireCrmSession("/customer-intelligence");
  const locale = await getRequestLocale();
  if (!crmCapabilityAllows(session.capabilities, "customers.duplicates.review")) {
    return denyAction(locale);
  }

  try {
    const subjectIdRaw = readOptionalString(formData, "subjectId");
    if (!subjectIdRaw || !isGuid(subjectIdRaw)) {
      return invalidField(locale, "subjectId");
    }
    const subjectId = subjectIdRaw;
    const entityType = readRequiredString(formData, "entityType");
    const options = await getCrmApiRequestOptions();
    const startedAt = performance.now();

    await crmApiClient.mutateOperationalEndpoint(
      "POST",
      "/api/customer-intelligence/duplicates/detect",
      {
        subjectId,
        entityType,
      },
      options,
    );
    customerIntelligenceMutationPerf.record("mutation.success", performance.now() - startedAt, {
      action: "detect-duplicates",
    });

    return {
      ...completeAction(subjectId),
      message: tCrm("crm.forms.result.completedDescription", locale),
    };
  } catch (error) {
    customerIntelligenceMutationPerf.record("mutation.error", 0, { action: "detect-duplicates" });
    return mapCrmMutationErrorToState(error, "/customer-intelligence");
  }
}

export async function mergeEntitiesFormAction(
  _previous: CrmMutationState = initialCrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  const session = await requireCrmSession("/customer-intelligence");
  const locale = await getRequestLocale();
  if (!crmCapabilityAllows(session.capabilities, "customers.duplicates.review")) {
    return denyAction(locale);
  }

  try {
    const primaryEntityIdRaw = readOptionalString(formData, "primaryEntityId");
    const secondaryEntityIdRaw = readOptionalString(formData, "secondaryEntityId");
    if (!primaryEntityIdRaw || !isGuid(primaryEntityIdRaw)) {
      return invalidField(locale, "primaryEntityId");
    }
    if (!secondaryEntityIdRaw || !isGuid(secondaryEntityIdRaw)) {
      return invalidField(locale, "secondaryEntityId");
    }
    const primaryEntityId = primaryEntityIdRaw;
    const secondaryEntityId = secondaryEntityIdRaw;
    const primaryEntityType = readRequiredString(formData, "primaryEntityType");
    const secondaryEntityType = readRequiredString(formData, "secondaryEntityType");
    const reason = readRequiredString(formData, "reason");
    const options = await getCrmApiRequestOptions();
    const startedAt = performance.now();

    await crmApiClient.mutateOperationalEndpoint(
      "POST",
      "/api/customer-intelligence/merges",
      {
        primaryEntityType,
        primaryEntityId,
        secondaryEntityType,
        secondaryEntityId,
        reason,
      },
      options,
    );
    customerIntelligenceMutationPerf.record("mutation.success", performance.now() - startedAt, {
      action: "merge-entities",
    });

    return {
      ...completeAction(primaryEntityId),
      message: tCrm("crm.forms.result.completedDescription", locale),
    };
  } catch (error) {
    customerIntelligenceMutationPerf.record("mutation.error", 0, { action: "merge-entities" });
    return mapCrmMutationErrorToState(error, "/customer-intelligence");
  }
}

export async function appendActivityFormAction(
  _previous: CrmMutationState = initialCrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  const session = await requireCrmSession("/customer-intelligence");
  const locale = await getRequestLocale();
  if (!crmCapabilityAllows(session.capabilities, "customerIntelligence.read")) {
    return denyAction(locale);
  }

  try {
    const subjectIdRaw = readOptionalString(formData, "subjectId");
    if (!subjectIdRaw || !isGuid(subjectIdRaw)) {
      return invalidField(locale, "subjectId");
    }
    const subjectId = subjectIdRaw;
    const name = readRequiredString(formData, "name");
    const category = readRequiredString(formData, "category");
    const channel = readOptionalString(formData, "channel");
    const options = await getCrmApiRequestOptions();
    const startedAt = performance.now();

    await crmApiClient.mutateOperationalEndpoint(
      "POST",
      "/api/customer-intelligence/activities",
      {
        subjectType: "Customer",
        subjectId,
        name,
        category,
        channel,
        entityType: "Customer",
        relatedEntityId: subjectId,
        dataJson: null,
        occurredAtUtc: null,
      },
      options,
    );
    customerIntelligenceMutationPerf.record("mutation.success", performance.now() - startedAt, {
      action: "append-activity",
    });

    return {
      ...completeAction(subjectId),
      message: tCrm("crm.forms.result.completedDescription", locale),
    };
  } catch (error) {
    customerIntelligenceMutationPerf.record("mutation.error", 0, { action: "append-activity" });
    return mapCrmMutationErrorToState(error, "/customer-intelligence");
  }
}

export async function upsertRelationshipFormAction(
  _previous: CrmMutationState = initialCrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  const session = await requireCrmSession("/customer-intelligence");
  const locale = await getRequestLocale();
  if (!crmCapabilityAllows(session.capabilities, "customerIntelligence.read")) {
    return denyAction(locale);
  }

  try {
    const sourceEntityIdRaw = readOptionalString(formData, "sourceEntityId");
    const targetEntityIdRaw = readOptionalString(formData, "targetEntityId");
    if (!sourceEntityIdRaw || !isGuid(sourceEntityIdRaw)) {
      return invalidField(locale, "sourceEntityId");
    }
    if (!targetEntityIdRaw || !isGuid(targetEntityIdRaw)) {
      return invalidField(locale, "targetEntityId");
    }
    const sourceEntityId = sourceEntityIdRaw;
    const targetEntityId = targetEntityIdRaw;
    const name = readRequiredString(formData, "name");
    const relationshipType = readRequiredString(formData, "relationshipType");
    const strengthScore = readOptionalDecimal(formData, "strengthScore") ?? 0.5;
    if (strengthScore < 0 || strengthScore > 1) {
      return invalidField(locale, "strengthScore");
    }
    const options = await getCrmApiRequestOptions();
    const startedAt = performance.now();

    await crmApiClient.mutateOperationalEndpoint(
      "PUT",
      "/api/customer-intelligence/relationships",
      {
        sourceEntityType: "Customer",
        sourceEntityId,
        targetEntityType: "Customer",
        targetEntityId,
        name,
        relationshipType,
        strengthScore,
        isBidirectional: true,
        dataJson: null,
      },
      options,
    );
    customerIntelligenceMutationPerf.record("mutation.success", performance.now() - startedAt, {
      action: "upsert-relationship",
    });

    return {
      ...completeAction(sourceEntityId),
      message: tCrm("crm.forms.result.completedDescription", locale),
    };
  } catch (error) {
    customerIntelligenceMutationPerf.record("mutation.error", 0, { action: "upsert-relationship" });
    return mapCrmMutationErrorToState(error, "/customer-intelligence");
  }
}

export async function trackCdpEventFormAction(
  _previous: CrmMutationState = initialCrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  const session = await requireCrmSession("/customer-intelligence");
  const locale = await getRequestLocale();
  if (!crmCapabilityAllows(session.capabilities, "customerIntelligence.read")) {
    return denyAction(locale);
  }

  try {
    const subjectIdRaw = readOptionalString(formData, "subjectId");
    if (!subjectIdRaw || !isGuid(subjectIdRaw)) {
      return invalidField(locale, "subjectId");
    }
    const subjectId = subjectIdRaw;
    const source = readRequiredString(formData, "source");
    const eventName = readRequiredString(formData, "eventName");
    const channel = readOptionalString(formData, "channel");
    const identityKey = readOptionalString(formData, "identityKey");
    const propertiesJson = readOptionalString(formData, "propertiesJson");
    if (propertiesJson) {
      try {
        JSON.parse(propertiesJson);
      } catch {
        return invalidField(locale, "propertiesJson");
      }
    }
    const options = await getCrmApiRequestOptions();
    const startedAt = performance.now();

    await crmApiClient.mutateOperationalEndpoint(
      "POST",
      "/api/customer-intelligence/cdp/events",
      {
        source,
        eventName,
        subjectType: "Customer",
        subjectId,
        identityKey,
        channel,
        propertiesJson,
        occurredAtUtc: null,
      },
      options,
    );
    customerIntelligenceMutationPerf.record("mutation.success", performance.now() - startedAt, {
      action: "track-cdp-event",
    });

    return {
      ...completeAction(subjectId),
      message: tCrm("crm.forms.result.completedDescription", locale),
    };
  } catch (error) {
    customerIntelligenceMutationPerf.record("mutation.error", 0, { action: "track-cdp-event" });
    return mapCrmMutationErrorToState(error, "/customer-intelligence");
  }
}

export async function resolveIdentityFormAction(
  _previous: CrmMutationState = initialCrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  const session = await requireCrmSession("/customer-intelligence");
  const locale = await getRequestLocale();
  if (!crmCapabilityAllows(session.capabilities, "customerIntelligence.read")) {
    return denyAction(locale);
  }

  try {
    const subjectIdRaw = readOptionalString(formData, "subjectId");
    if (!subjectIdRaw || !isGuid(subjectIdRaw)) {
      return invalidField(locale, "subjectId");
    }
    const subjectId = subjectIdRaw;
    const identityType = readRequiredString(formData, "identityType");
    const identityValue = readRequiredString(formData, "identityValue");
    const confidenceScore = readOptionalDecimal(formData, "confidenceScore") ?? 0.8;
    if (confidenceScore < 0 || confidenceScore > 1) {
      return invalidField(locale, "confidenceScore");
    }
    const resolutionNotes = readOptionalString(formData, "resolutionNotes");
    const options = await getCrmApiRequestOptions();
    const startedAt = performance.now();

    await crmApiClient.mutateOperationalEndpoint(
      "POST",
      "/api/customer-intelligence/cdp/identity-resolution",
      {
        subjectType: "Customer",
        subjectId,
        identityType,
        identityValue,
        confidenceScore,
        resolutionNotes,
      },
      options,
    );
    customerIntelligenceMutationPerf.record("mutation.success", performance.now() - startedAt, {
      action: "resolve-identity",
    });

    return {
      ...completeAction(subjectId),
      message: tCrm("crm.forms.result.completedDescription", locale),
    };
  } catch (error) {
    customerIntelligenceMutationPerf.record("mutation.error", 0, { action: "resolve-identity" });
    return mapCrmMutationErrorToState(error, "/customer-intelligence");
  }
}
