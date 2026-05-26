import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";

const securityHeaders = {
  "X-Frame-Options": "DENY",
  "X-Content-Type-Options": "nosniff",
  "Referrer-Policy": "strict-origin-when-cross-origin",
  "Permissions-Policy": "camera=(), microphone=(), geolocation=()",
} as const;

export function proxy(request: NextRequest) {
  const response = NextResponse.next();

  for (const [header, value] of Object.entries(securityHeaders)) {
    response.headers.set(header, value);
  }

  const pathname = request.nextUrl.pathname;

  if (pathname === "/") {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  return response;
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|robots.txt|sitemap.xml|.well-known).*)"],
};
