import "server-only";

import type {
  MfaStatusResponse,
  TrustedDevicesResponse,
  UserSessionsResponse,
} from "@/lib/account-api";
import { accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { getOverviewForPage } from "@/features/account/data/account-read-data";

export async function getSecurityOverviewForPage(): Promise<{
  overview: Awaited<ReturnType<typeof getOverviewForPage>>;
  mfaStatus: MfaStatusResponse;
  sessions: UserSessionsResponse;
  trustedDevices: TrustedDevicesResponse;
}> {
  const requestOptions = await getAccountApiRequestOptions();

  const [overview, mfaStatus, sessions, trustedDevices] = await Promise.all([
    getOverviewForPage(),
    accountApiClient.getMfaStatus(requestOptions),
    accountApiClient.getSessions(requestOptions),
    accountApiClient.getTrustedDevices(requestOptions),
  ]);

  return { overview, mfaStatus, sessions, trustedDevices };
}

export async function getSessionsAndDevicesForPage(): Promise<{
  sessions: UserSessionsResponse;
  trustedDevices: TrustedDevicesResponse;
}> {
  const requestOptions = await getAccountApiRequestOptions();

  const [sessions, trustedDevices] = await Promise.all([
    accountApiClient.getSessions(requestOptions),
    accountApiClient.getTrustedDevices(requestOptions),
  ]);

  return { sessions, trustedDevices };
}

export async function getMfaStatusForPage(): Promise<MfaStatusResponse> {
  const requestOptions = await getAccountApiRequestOptions();
  return accountApiClient.getMfaStatus(requestOptions);
}
