"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient, type PipelineStageRequest } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { assertSameOriginRequest } from "@/lib/security/csrf";

function readOptionalString(formData: FormData, field: string): string | null {
  const value = formData.get(field);
  return typeof value === "string" ? emptyToNull(value) : null;
}

function readRequiredString(formData: FormData, field: string): string {
  const value = readOptionalString(formData, field);
  if (!value) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readRequiredInteger(formData: FormData, field: string): number {
  const value = Number(formData.get(field));
  if (!Number.isInteger(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readRequiredDecimal(formData: FormData, field: string): number {
  const value = Number(formData.get(field));
  if (Number.isNaN(value)) {
    throw new Error(`Invalid ${field}.`);
  }

  return value;
}

function readPipelineStages(formData: FormData): PipelineStageRequest[] {
  const raw = readOptionalString(formData, "stagesJson");
  if (!raw) {
    return [
      {
        name: readRequiredString(formData, "stageName"),
        description: readOptionalString(formData, "stageDescription"),
        displayOrder: readRequiredInteger(formData, "stageDisplayOrder"),
        probability: readRequiredDecimal(formData, "stageProbability"),
        isWinStage: formData.get("stageIsWin") === "true",
        isLostStage: formData.get("stageIsLost") === "true",
      },
    ];
  }

  const parsed = JSON.parse(raw) as PipelineStageRequest[];
  if (!Array.isArray(parsed) || parsed.length === 0) {
    throw new Error("Invalid pipeline stages.");
  }

  return parsed;
}

export async function createPipelineFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/pipeline", "pipeline.manage");

  const options = await getCrmApiRequestOptions();
  const pipeline = await crmApiClient.createPipeline(
    {
      name: readRequiredString(formData, "name"),
      description: readOptionalString(formData, "description"),
      isDefault: formData.get("isDefault") === "true",
      displayOrder: readRequiredInteger(formData, "displayOrder"),
      stages: readPipelineStages(formData),
    },
    options,
  );

  revalidatePath("/pipeline");
  redirect(`/pipeline?pipelineId=${pipeline.id}`);
}

export async function updatePipelineFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/pipeline", "pipeline.manage");

  const pipelineId = readRequiredString(formData, "pipelineId");
  if (!isGuid(pipelineId)) {
    throw new Error("Invalid pipeline.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.updatePipeline(
    pipelineId,
    {
      id: pipelineId,
      name: readRequiredString(formData, "name"),
      description: readOptionalString(formData, "description"),
      isDefault: formData.get("isDefault") === "true",
      displayOrder: readRequiredInteger(formData, "displayOrder"),
      stages: readPipelineStages(formData),
      rowVersion: readRequiredString(formData, "rowVersion"),
    },
    options,
  );

  revalidatePath("/pipeline");
  redirect(`/pipeline?pipelineId=${pipelineId}`);
}

export async function deletePipelineFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/pipeline", "pipeline.manage");

  const pipelineId = readRequiredString(formData, "pipelineId");
  if (!isGuid(pipelineId) || formData.get("confirm") !== "delete-pipeline") {
    throw new Error("Invalid pipeline delete request.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.deletePipeline(pipelineId, options);

  revalidatePath("/pipeline");
  redirect("/pipeline");
}

export async function createPipelineLostReasonFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/pipeline", "pipelineLostReasons.manage");

  const options = await getCrmApiRequestOptions();
  await crmApiClient.createPipelineLostReason(
    {
      name: readRequiredString(formData, "name"),
      description: readOptionalString(formData, "description"),
      isDefault: formData.get("isDefault") === "true",
    },
    options,
  );

  revalidatePath("/pipeline");
  redirect("/pipeline");
}

export async function updatePipelineLostReasonFormAction(formData: FormData): Promise<void> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/pipeline", "pipelineLostReasons.manage");

  const lostReasonId = readRequiredString(formData, "lostReasonId");
  if (!isGuid(lostReasonId)) {
    throw new Error("Invalid lost reason.");
  }

  const options = await getCrmApiRequestOptions();
  await crmApiClient.updatePipelineLostReason(
    lostReasonId,
    {
      name: readRequiredString(formData, "name"),
      description: readOptionalString(formData, "description"),
      isDefault: formData.get("isDefault") === "true",
      rowVersion: readRequiredString(formData, "rowVersion"),
    },
    options,
  );

  revalidatePath("/pipeline");
  redirect("/pipeline");
}
