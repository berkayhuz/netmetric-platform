import "server-only";

import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { getAccessTokenFromCookieHeader } from "@netmetric/auth";

import { serverEnv } from "@/lib/env/server-env";

type ProxyMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

type ProxyOptions = {
  endpoint: string;
  method: ProxyMethod;
};

type FetchErrorWithCause = Error & {
  cause?: {
    code?: string;
  };
};

const accessCookieEnv = process.env.AUTH_ACCESS_COOKIE_NAME?.trim();

function getAuthAccessCookieNames(): readonly string[] {
  return accessCookieEnv ? [accessCookieEnv] : [];
}

function getCorrelationId(request: NextRequest): string | null {
  return request.headers.get("x-request-id") ?? request.headers.get("x-correlation-id");
}

function buildGatewayUrl(endpoint: string): string {
  const normalizedEndpoint = endpoint.startsWith("/") ? endpoint : `/${endpoint}`;
  const gatewayBaseUrl =
    serverEnv.NEXT_PUBLIC_API_GATEWAY_BASE_URL ?? serverEnv.NEXT_PUBLIC_API_BASE_URL;

  return `${gatewayBaseUrl}${normalizedEndpoint}`;
}

function getSetCookieHeaders(headers: Headers): string[] {
  const withGetSetCookie = headers as Headers & {
    getSetCookie?: () => string[];
  };

  const cookies = withGetSetCookie.getSetCookie?.();

  if (cookies && cookies.length > 0) {
    return cookies;
  }

  const singleCookie = headers.get("set-cookie");

  return singleCookie ? [singleCookie] : [];
}

function copyResponseHeaders(upstream: Response, response: NextResponse): void {
  const contentType = upstream.headers.get("content-type");
  const cacheControl = upstream.headers.get("cache-control");

  if (contentType) {
    response.headers.set("content-type", contentType);
  }

  if (cacheControl) {
    response.headers.set("cache-control", cacheControl);
  }

  for (const cookie of getSetCookieHeaders(upstream.headers)) {
    response.headers.append("set-cookie", cookie);
  }
}

export async function proxyToGateway(
  request: NextRequest,
  options: ProxyOptions,
): Promise<NextResponse> {
  const correlationId = getCorrelationId(request);
  const requestHeaders = new Headers();

  requestHeaders.set("accept", "application/json");

  const contentType = request.headers.get("content-type");

  if (contentType) {
    requestHeaders.set("content-type", contentType);
  }

  const cookie = request.headers.get("cookie");

  if (cookie) {
    requestHeaders.set("cookie", cookie);
    const accessToken = getAccessTokenFromCookieHeader(cookie, getAuthAccessCookieNames());
    if (accessToken) {
      requestHeaders.set("authorization", `Bearer ${accessToken}`);
    }
  }

  const origin = request.headers.get("origin") ?? request.nextUrl.origin;

  if (origin) {
    requestHeaders.set("origin", origin);
  }

  const referer = request.headers.get("referer");

  if (referer) {
    requestHeaders.set("referer", referer);
  }

  const userAgent = request.headers.get("user-agent");

  if (userAgent) {
    requestHeaders.set("user-agent", userAgent);
  }

  if (correlationId) {
    requestHeaders.set("x-request-id", correlationId);
  }

  const body =
    options.method === "GET" || options.method === "DELETE" ? undefined : await request.text();

  const requestInit: RequestInit = {
    method: options.method,
    headers: requestHeaders,
    cache: "no-store",
    redirect: "manual",
  };

  if (body && body.length > 0) {
    requestInit.body = body;
  }

  let upstream: Response;

  try {
    upstream = await fetch(buildGatewayUrl(options.endpoint), requestInit);
  } catch (error) {
    const fetchError = error as FetchErrorWithCause;
    const code = fetchError.cause?.code;
    const isUpstreamUnavailable = code === "ECONNREFUSED" || code === "EHOSTUNREACH";
    const status = isUpstreamUnavailable ? 503 : 502;

    const response = NextResponse.json(
      {
        type: "about:blank",
        title: "Gateway unavailable",
        status,
        detail: "Auth service is temporarily unavailable. Please try again shortly.",
        errorCode: "upstream_unavailable",
        correlationId: correlationId ?? undefined,
      },
      { status },
    );

    if (correlationId) {
      response.headers.set("x-request-id", correlationId);
    }

    return response;
  }

  const responseBody = await upstream.text();

  const response = new NextResponse(responseBody || null, {
    status: upstream.status,
    statusText: upstream.statusText,
  });

  copyResponseHeaders(upstream, response);

  if (correlationId) {
    response.headers.set("x-request-id", correlationId);
  }

  return response;
}
