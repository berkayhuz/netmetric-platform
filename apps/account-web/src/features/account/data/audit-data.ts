import "server-only";

import { accountApiClient, type AccountAuditEntriesResponse } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";

export type AuditFilter = {
  limit?: number;
  eventType?: string;
};

export async function getAuditEntriesForPage(
  filter: AuditFilter = {},
): Promise<AccountAuditEntriesResponse> {
  const requestOptions = await getAccountApiRequestOptions();
  return accountApiClient.getAuditEntries(filter, requestOptions);
}
