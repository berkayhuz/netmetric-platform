import "server-only";

import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import {
  createAuthServiceHeaders,
  getCorrelationIdFromHeaders,
  getSetCookieHeaders,
} from "@netmetric/auth";

function getGatewayBaseUrl(): string {
  return (
    process.env.ACCOUNT_API_BASE_URL ??
    process.env.NEXT_PUBLIC_API_BASE_URL ??
    "http://localhost:5030"
  ).replace(/\/+$/, "");
}

export async function proxyAuthToGateway(
  request: NextRequest,
  endpoint: string,
  method: "DELETE" | "GET" | "POST",
  body?: string,
): Promise<NextResponse> {
  const headers = createAuthServiceHeaders({
    cookieHeader: request.headers.get("cookie"),
    correlationId: getCorrelationIdFromHeaders(request.headers),
    origin: request.headers.get("origin") ?? request.nextUrl.origin,
    referer: request.headers.get("referer"),
    userAgent: request.headers.get("user-agent"),
    contentType: request.headers.get("content-type"),
  });

  const requestInit: RequestInit = {
    method,
    headers,
    cache: "no-store",
    redirect: "manual",
  };

  if (method === "POST" || method === "DELETE") {
    requestInit.body = body ?? (await request.text());
  }

  const upstream = await fetch(`${getGatewayBaseUrl()}${endpoint}`, requestInit);

  const response = new NextResponse((await upstream.text()) || null, {
    status: upstream.status,
    statusText: upstream.statusText,
  });

  const upstreamContentType = upstream.headers.get("content-type");
  if (upstreamContentType) {
    response.headers.set("content-type", upstreamContentType);
  }

  for (const setCookie of getSetCookieHeaders(upstream.headers)) {
    response.headers.append("set-cookie", setCookie);
  }

  return response;
}
