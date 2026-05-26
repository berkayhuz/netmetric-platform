import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import {
  createAuthLogoutRequestHeaders,
  getCorrelationIdFromHeaders,
  logoutFromAuthService,
} from "@netmetric/auth";

import { crmEnv, crmServerEnv } from "@/lib/crm-env";
import { clearCrmAuthCookies } from "@/lib/crm-auth/auth-cookie-clear";

export function createLogoutRequestHeaders(request: NextRequest): Headers {
  return createAuthLogoutRequestHeaders({
    cookieHeader: request.headers.get("cookie"),
    correlationId: getCorrelationIdFromHeaders(request.headers),
    origin: request.headers.get("origin") ?? request.nextUrl.origin,
    referer: request.headers.get("referer"),
    userAgent: request.headers.get("user-agent"),
    contentType: "application/json",
  });
}

export async function POST(request: NextRequest): Promise<NextResponse> {
  const loginUrl = new URL("/login", crmEnv.authUrl).toString();
  const response = NextResponse.json({ redirectUrl: loginUrl });
  clearCrmAuthCookies(request, response);

  const logout = await logoutFromAuthService({
    authBaseUrl: crmServerEnv.authSessionBaseUrl,
    cookieHeader: request.headers.get("cookie"),
    correlationId: getCorrelationIdFromHeaders(request.headers),
    origin: request.headers.get("origin") ?? request.nextUrl.origin,
    referer: request.headers.get("referer"),
    userAgent: request.headers.get("user-agent"),
  });

  for (const cookie of logout.setCookieHeaders) {
    response.headers.append("set-cookie", cookie);
  }

  if (logout.unavailable) {
    response.headers.set("x-netmetric-auth-logout", "unavailable");
  }

  return response;
}
