import type { NextRequest, NextResponse } from "next/server";
import { getPreferenceCookieOptions } from "@netmetric/i18n";
import { createAuthCookieDescriptors, shouldUseSecureAuthCookie } from "@netmetric/auth";

export function clearAccountAuthCookies(request: NextRequest, response: NextResponse): void {
  const cookieOptions = getPreferenceCookieOptions({
    appOrigin: request.nextUrl.origin,
    cookieDomain: process.env.NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN,
  });

  for (const cookie of createAuthCookieDescriptors({
    accessCookieName: process.env.ACCOUNT_ACCESS_COOKIE_NAME,
    refreshCookieName: process.env.ACCOUNT_REFRESH_COOKIE_NAME,
    sessionCookieName: process.env.ACCOUNT_SESSION_COOKIE_NAME,
  })) {
    response.cookies.set({
      name: cookie.name,
      value: "",
      expires: new Date(0),
      maxAge: 0,
      httpOnly: true,
      secure: shouldUseSecureAuthCookie(cookie.name, cookieOptions.secure),
      sameSite: cookieOptions.sameSite,
      path: cookie.path,
      domain: cookieOptions.domain,
    });
  }
}
