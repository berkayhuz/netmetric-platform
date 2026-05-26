"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { opportunityStageOptions } from "@/features/shared/forms/options";
import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { assertSameOriginRequest } from "@/lib/security/csrf";

const allowedStages = new Set<number>(opportunityStageOptions.map((item) => item.value));

export async function moveOpportunityStageAction(
  opportunityId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/pipeline", "pipeline.manage");

  if (!isGuid(opportunityId)) {
    return { status: "error", message: tCrm("crm.pipeline.validation.invalidOpportunityId") };
  }

  const stageRaw = formData.get("newStage");
  const stageNumber = typeof stageRaw === "string" ? Number(stageRaw) : Number.NaN;

  if (!Number.isInteger(stageNumber) || !allowedStages.has(stageNumber)) {
    return {
      status: "error",
      message: tCrm("crm.pipeline.validation.chooseValidStage"),
      fieldErrors: { newStage: [tCrm("crm.pipeline.validation.invalidTargetStage")] },
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.moveOpportunityStage(
      opportunityId,
      {
        newStage: stageNumber,
        newPipelineStageId: null,
        note: null,
        lostReasonId: null,
        lostNote: null,
        rowVersion: null,
      },
      options,
    );

    revalidatePath("/pipeline");
    revalidatePath("/opportunities");
    revalidatePath(`/opportunities/${opportunityId}`);

    return {
      status: "success",
      message: tCrm("crm.pipeline.result.stageUpdated"),
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/pipeline");
  }
}
