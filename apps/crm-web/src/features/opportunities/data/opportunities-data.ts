import "server-only";

import { crmApiClient, type ActivityTimelineFeed, type CrmListQuery } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getOpportunitiesData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listOpportunities(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getOpportunityDetailData(opportunityId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getOpportunityById(opportunityId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getOpportunityWorkspaceData(opportunityId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getOpportunityWorkspace(opportunityId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getOpportunityTimelineData(opportunityId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getOpportunityTimeline(opportunityId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getOpportunityLostReasonsData(returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listOpportunityLostReasons(options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getOpportunityQuotesData(opportunityId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listOpportunityQuotes(opportunityId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getPipelineOpportunityStageHistoryData(
  opportunityId: string,
  returnPath: string,
) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getPipelineOpportunityStageHistory(opportunityId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getOpportunityActivitiesTimelineData(
  opportunityId: string,
  returnPath: string,
  query: {
    from?: string;
    to?: string;
    page?: number;
    pageSize?: number;
  } = {},
): Promise<ActivityTimelineFeed> {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listRelatedActivities("opportunity", opportunityId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
