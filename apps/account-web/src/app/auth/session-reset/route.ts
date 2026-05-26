import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";

import { clearAccountAuthCookies } from "@/lib/auth/auth-cookie-clear";
import { buildAuthLoginRedirectUrl } from "@/lib/auth/safe-return-url";

export function GET(request: NextRequest): NextResponse {
  const returnUrl = request.nextUrl.searchParams.get("returnUrl");
  const response = NextResponse.redirect(buildAuthLoginRedirectUrl(returnUrl ?? undefined));
  clearAccountAuthCookies(request, response);
  response.headers.set("cache-control", "no-store");
  response.headers.set("x-netmetric-auth-session-reset", "1");

  return response;
}
