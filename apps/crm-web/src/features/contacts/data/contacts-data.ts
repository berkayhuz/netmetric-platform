import "server-only";

import { crmApiClient, type ActivityTimelineFeed, type CrmListQuery } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getContactsData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listContacts(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getContactDetailData(contactId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getContactById(contactId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getContactActivitiesTimelineData(
  contactId: string,
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

    return await crmApiClient.listRelatedActivities("contact", contactId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
