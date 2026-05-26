import "server-only";

import { crmApiClient, type ActivityTimelineFeed, type CrmListQuery } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getTicketsData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listTickets(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getTicketDetailData(ticketId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getTicketById(ticketId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getTicketActivitiesTimelineData(
  ticketId: string,
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

    return await crmApiClient.listRelatedActivities("ticket", ticketId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
