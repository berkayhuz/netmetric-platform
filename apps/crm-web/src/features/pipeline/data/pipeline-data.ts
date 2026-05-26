import "server-only";

import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

async function getOptions() {
  return getCrmApiRequestOptions();
}

export async function getPipelinesData(returnPath: string) {
  try {
    const options = await getOptions();
    return await crmApiClient.listPipelines(options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getPipelineDetailData(pipelineId: string, returnPath: string) {
  try {
    const options = await getOptions();
    return await crmApiClient.getPipelineById(pipelineId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getPipelineBoardData(
  pipelineId: string,
  returnPath: string,
  ownerUserId?: string,
) {
  try {
    const options = await getOptions();
    return await crmApiClient.getPipelineBoard(
      pipelineId,
      ownerUserId ? { ownerUserId } : {},
      options,
    );
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getPipelineAnalyticsData(pipelineId: string, returnPath: string) {
  try {
    const options = await getOptions();
    return await crmApiClient.getPipelineAnalytics(pipelineId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getPipelineLostReasonsData(returnPath: string) {
  try {
    const options = await getOptions();
    return await crmApiClient.listPipelineLostReasons(options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
