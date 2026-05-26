import { fetchAuthJson } from "@netmetric/auth";
import { headers } from "next/headers";
import { NextResponse } from "next/server";

import type { DashboardCollection } from "@/features/widgets/types";
import { crmEnv } from "@/lib/crm-env";
import {
  getCrmAccessCookieNames,
  getCrmAccessToken,
  getRequestCorrelationId,
} from "@/lib/crm-auth/crm-auth-headers";

type AccountPreferencesResponse = {
  id: string;
  theme: string;
  language: string;
  timeZone: string;
  dateFormat: string;
  postLoginDestination: string;
  defaultOrganizationId?: string | null;
  faviconUrl?: string | null;
  crmDashboardPreferencesJson?: string | null;
  version: string;
};

type AccountPreferencesUpdateRequest = {
  theme: string;
  language: string;
  timeZone: string;
  dateFormat: string;
  postLoginDestination: string;
  defaultOrganizationId?: string | null;
  crmDashboardPreferencesJson?: string | null;
  version?: string | null;
};

function accountPreferencesUrl(): string {
  return new URL("/api/v1/account/preferences", crmEnv.accountApiBaseUrl).toString();
}

async function loadAccountPreferences(): Promise<AccountPreferencesResponse | null> {
  const headerStore = await headers();
  const accessToken = await getCrmAccessToken();
  if (!accessToken) {
    return null;
  }

  return fetchAuthJson<AccountPreferencesResponse>({
    authBaseUrl: crmEnv.accountApiBaseUrl,
    path: "/api/v1/account/preferences",
    accessToken,
    accessCookieNames: getCrmAccessCookieNames(),
    cookieHeader: headerStore.get("cookie"),
    correlationId: await getRequestCorrelationId(),
  });
}

export async function GET() {
  const preferences = await loadAccountPreferences();
  if (!preferences) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }

  return NextResponse.json({
    crmDashboardPreferencesJson: preferences.crmDashboardPreferencesJson ?? null,
    version: preferences.version,
  });
}

export async function PUT(request: Request) {
  const preferences = await loadAccountPreferences();
  if (!preferences) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }

  const body = (await request.json()) as { collection?: DashboardCollection };
  if (!body.collection) {
    return NextResponse.json({ message: "Missing collection payload" }, { status: 400 });
  }

  const updateRequest: AccountPreferencesUpdateRequest = {
    theme: preferences.theme,
    language: preferences.language,
    timeZone: preferences.timeZone,
    dateFormat: preferences.dateFormat,
    postLoginDestination: preferences.postLoginDestination,
    defaultOrganizationId: preferences.defaultOrganizationId ?? null,
    crmDashboardPreferencesJson: JSON.stringify(body.collection),
    version: preferences.version,
  };

  const headerStore = await headers();
  const accessToken = await getCrmAccessToken();
  const correlationId = await getRequestCorrelationId();

  const response = await fetch(accountPreferencesUrl(), {
    method: "PUT",
    headers: {
      accept: "application/json",
      "content-type": "application/json",
      ...(accessToken ? { authorization: `Bearer ${accessToken}` } : {}),
      ...(headerStore.get("cookie") ? { cookie: headerStore.get("cookie") as string } : {}),
      ...(correlationId
        ? { "x-request-id": correlationId, "x-correlation-id": correlationId }
        : {}),
    },
    body: JSON.stringify(updateRequest),
    cache: "no-store",
    redirect: "manual",
  });

  if (!response.ok) {
    const text = await response.text();
    return NextResponse.json(
      { message: "Failed to persist", detail: text },
      { status: response.status },
    );
  }

  const payload = (await response.json()) as AccountPreferencesResponse;
  return NextResponse.json({
    crmDashboardPreferencesJson: payload.crmDashboardPreferencesJson ?? null,
    version: payload.version,
  });
}
