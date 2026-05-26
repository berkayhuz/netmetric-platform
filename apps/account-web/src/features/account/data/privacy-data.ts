import "server-only";

import { accountApiClient, type ConsentsResponse } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";

export async function getConsentsForPage(): Promise<ConsentsResponse> {
  const requestOptions = await getAccountApiRequestOptions();
  return accountApiClient.getConsents(requestOptions);
}
