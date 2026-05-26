import "server-only";

import { crmApiClient, type ActivityTimelineFeed, type CrmListQuery } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getQuotesData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listQuotes(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getQuoteDetailData(quoteId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getQuoteById(quoteId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getQuoteWorkspaceData(quoteId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getQuoteWorkspace(quoteId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getQuoteTimelineData(quoteId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getQuoteTimeline(quoteId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getQuoteValidationData(quoteId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.validateQuoteConfiguration(quoteId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getQuoteCpqWorkspaceData(returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getQuoteCpqWorkspace(options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getProposalTemplatesData(returnPath: string, isActive?: boolean | null) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listProposalTemplates(isActive, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getQuoteActivitiesTimelineData(
  quoteId: string,
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

    return await crmApiClient.listRelatedActivities("quote", quoteId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
