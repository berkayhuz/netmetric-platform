import type { NextRequest } from "next/server";

import { proxyAuthToGateway } from "@/lib/auth/auth-gateway-proxy";

export async function GET(request: NextRequest) {
  return proxyAuthToGateway(request, "/api/auth/workspaces", "GET");
}

export async function POST(request: NextRequest) {
  const body = await request.text();
  let payload: { intent?: unknown; tenantId?: unknown } | null = null;
  try {
    payload = JSON.parse(body || "null") as { intent?: unknown; tenantId?: unknown } | null;
  } catch {
    payload = null;
  }
  if (payload?.intent === "delete") {
    const tenantId = typeof payload.tenantId === "string" ? payload.tenantId.trim() : "";
    if (!tenantId) {
      return Response.json(
        { errorCode: "invalid_workspace", message: "Workspace id is required." },
        { status: 400 },
      );
    }

    return proxyAuthToGateway(
      request,
      `/api/auth/workspaces/${encodeURIComponent(tenantId)}`,
      "DELETE",
      body,
    );
  }

  return proxyAuthToGateway(request, "/api/auth/workspaces", "POST", body);
}

export async function DELETE(request: NextRequest) {
  const payload = (await request.json().catch(() => null)) as { tenantId?: unknown } | null;
  const tenantId = typeof payload?.tenantId === "string" ? payload.tenantId.trim() : "";

  if (!tenantId) {
    return Response.json(
      { errorCode: "invalid_workspace", message: "Workspace id is required." },
      { status: 400 },
    );
  }

  return proxyAuthToGateway(
    request,
    `/api/auth/workspaces/${encodeURIComponent(tenantId)}`,
    "DELETE",
    JSON.stringify({ tenantId }),
  );
}
