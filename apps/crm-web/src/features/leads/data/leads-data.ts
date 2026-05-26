import "server-only";

import { crmApiClient, type ActivityTimelineFeed, type CrmListQuery } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getLeadsData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listLeads(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getLeadDetailData(leadId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getLeadById(leadId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getLeadWorkspaceData(leadId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getLeadWorkspace(leadId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getLeadTimelineData(leadId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getLeadTimeline(leadId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getLeadActivitiesTimelineData(
  leadId: string,
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

    return await crmApiClient.listRelatedActivities("lead", leadId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
