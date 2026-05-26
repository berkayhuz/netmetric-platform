import "server-only";

import { crmApiClient, type ActivityTimelineFeed, type CrmListQuery } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getCompaniesData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listCompanies(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCompanyDetailData(companyId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getCompanyById(companyId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCompanyActivitiesTimelineData(
  companyId: string,
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

    return await crmApiClient.listRelatedActivities("company", companyId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
