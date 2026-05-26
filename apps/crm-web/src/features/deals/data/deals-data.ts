import "server-only";

import {
  crmApiClient,
  type ActivityTimelineFeed,
  type CrmListQuery,
  type DealWinLossSummaryQuery,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getDealsData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listDeals(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getDealDetailData(dealId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getDealById(dealId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getDealWorkspaceData(dealId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getDealWorkspace(dealId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getDealTimelineData(dealId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getDealTimeline(dealId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getDealWinLossSummaryData(
  query: DealWinLossSummaryQuery,
  returnPath: string,
) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getDealWinLossSummary(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getDealLostReasonsData(returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listDealLostReasons(options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getDealActivitiesTimelineData(
  dealId: string,
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

    return await crmApiClient.listRelatedActivities("deal", dealId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
