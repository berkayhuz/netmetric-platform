import "server-only";

import { headers } from "next/headers";
import { fetchAuthJson } from "@netmetric/auth";

import { crmEnv } from "@/lib/crm-env";
import {
  getCrmAccessCookieNames,
  getCrmAccessToken,
  getRequestCorrelationId,
} from "@/lib/crm-auth/crm-auth-headers";
import { createDefaultDashboardProfile } from "@/features/widgets/registry/widget-catalog";
import type { DashboardCollection } from "@/features/widgets/types";

type AccountPreferencesResponse = {
  crmDashboardPreferencesJson?: string | null;
};

function createInitialCollection(): DashboardCollection {
  const profile = createDefaultDashboardProfile();
  return {
    active: [profile],
    trash: [],
    selectedDashboardId: profile.id,
    updatedAt: "1970-01-01T00:00:00.000Z",
  };
}

export async function getServerDashboardCollection(): Promise<DashboardCollection> {
  const headerStore = await headers();
  const accessToken = await getCrmAccessToken();

  if (!accessToken) {
    return createInitialCollection();
  }

  const payload = await fetchAuthJson<AccountPreferencesResponse>({
    authBaseUrl: crmEnv.accountApiBaseUrl,
    path: "/api/v1/account/preferences",
    accessToken,
    accessCookieNames: getCrmAccessCookieNames(),
    cookieHeader: headerStore.get("cookie"),
    correlationId: await getRequestCorrelationId(),
  });

  if (!payload?.crmDashboardPreferencesJson) {
    return createInitialCollection();
  }

  try {
    return JSON.parse(payload.crmDashboardPreferencesJson) as DashboardCollection;
  } catch {
    return createInitialCollection();
  }
}
