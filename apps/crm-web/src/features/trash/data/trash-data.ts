import "server-only";

import { crmApiClient, type CrmListQuery } from "@/lib/crm-api";
import { CrmApiError } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getTrashData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();
    return await crmApiClient.listTrashItems(query, options);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      const pageNumber = Math.max(1, query.page ?? 1);
      const pageSize = Math.max(1, query.pageSize ?? 20);
      return {
        items: [],
        totalCount: 0,
        pageNumber,
        pageSize,
        totalPages: 0,
      };
    }
    handleCrmApiPageError(error, returnPath);
  }
}
