import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { getCorrelationIdFromHeaders, logoutFromAuthService } from "@netmetric/auth";

import { toolsEnv } from "@/lib/tools-env";

export async function POST(request: NextRequest): Promise<NextResponse> {
  const response = NextResponse.redirect(new URL("/", request.url), { status: 303 });
  const logout = await logoutFromAuthService({
    authBaseUrl: toolsEnv.apiBaseUrl,
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
