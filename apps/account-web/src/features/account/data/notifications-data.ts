import "server-only";

import {
  accountApiClient,
  type AccountNotificationsResponse,
  type NotificationPreferencesResponse,
} from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";

export async function getNotificationsForPage(
  filter: string | undefined,
): Promise<AccountNotificationsResponse> {
  const requestOptions = await getAccountApiRequestOptions();
  return accountApiClient.getNotifications(filter, requestOptions);
}

export async function getNotificationPreferencesForPage(): Promise<NotificationPreferencesResponse> {
  const requestOptions = await getAccountApiRequestOptions();
  return accountApiClient.getNotificationPreferences(requestOptions);
}
