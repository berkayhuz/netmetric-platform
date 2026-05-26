import { NextResponse, type NextRequest } from "next/server";

const securityHeaders = {
  "X-Frame-Options": "DENY",
  "X-Content-Type-Options": "nosniff",
  "Referrer-Policy": "strict-origin-when-cross-origin",
  "Permissions-Policy": "camera=(), microphone=(), geolocation=()",
  "Cross-Origin-Opener-Policy": "same-origin",
  "Cross-Origin-Resource-Policy": "same-site",
  "X-DNS-Prefetch-Control": "off",
  "X-Permitted-Cross-Domain-Policies": "none",
} as const;

export function proxy(request: NextRequest) {
  const requestHeaders = new Headers(request.headers);
  requestHeaders.set("x-netmetric-pathname", request.nextUrl.pathname);
  const response = NextResponse.next({
    request: {
      headers: requestHeaders,
    },
  });

  for (const [header, value] of Object.entries(securityHeaders)) {
    response.headers.set(header, value);
  }

  return response;
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
