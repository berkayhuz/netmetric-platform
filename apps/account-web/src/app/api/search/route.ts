import type { AccountApiAuthContext } from "@/lib/account-api";
import { getAccountApiConfig } from "@/lib/account-api";
import { getAccountApiAuthContext, getRequestCorrelationId } from "@/lib/auth/account-auth-headers";
import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";

const forwardedSearchParams = new Set([
  "q",
  "query",
  "source",
  "sources",
  "type",
  "locale",
  "tags",
  "page",
  "pageSize",
  "sort",
]);

export function createSearchProxyUrl(request: NextRequest, baseUrl: string): string {
  const target = new URL("/api/v1/search", baseUrl.replace(/\/+$/, ""));

  for (const [key, value] of request.nextUrl.searchParams) {
    if (forwardedSearchParams.has(key)) {
      target.searchParams.append(key, value);
    }
  }

  return target.toString();
}

export function createSearchProxyHeaders(
  authContext: AccountApiAuthContext | undefined,
  correlationId: string | undefined,
): Headers {
  const headers = new Headers();
  headers.set("accept", "application/json");

  if (authContext?.bearerToken) {
    headers.set("authorization", `Bearer ${authContext.bearerToken}`);
  }

  if (correlationId) {
    headers.set("x-correlation-id", correlationId);
  }

  return headers;
}

export async function GET(request: NextRequest): Promise<NextResponse> {
  try {
    const [authContext, correlationId] = await Promise.all([
      getAccountApiAuthContext(),
      getRequestCorrelationId(),
    ]);
    const upstream = await fetch(createSearchProxyUrl(request, getAccountApiConfig().baseUrl), {
      method: "GET",
      headers: createSearchProxyHeaders(authContext, correlationId),
      cache: "no-store",
      redirect: "manual",
      signal: AbortSignal.timeout(10_000),
    });
    const body = await upstream.text();
    const response = new NextResponse(body || null, {
      status: upstream.status,
      statusText: upstream.statusText,
    });
    const contentType = upstream.headers.get("content-type");

    response.headers.set("cache-control", "private, no-store");
    if (contentType) {
      response.headers.set("content-type", contentType);
    }

    return response;
  } catch {
    return NextResponse.json(
      {
        error: "Search is unavailable right now.",
      },
      {
        status: 503,
        headers: {
          "cache-control": "private, no-store",
        },
      },
    );
  }
}
