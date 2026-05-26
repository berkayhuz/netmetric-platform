import type { NextRequest } from "next/server";

import { proxyAuthToGateway } from "@/lib/auth/auth-gateway-proxy";

type RouteContext = {
  params: Promise<{
    tenantId: string;
  }>;
};

export async function DELETE(request: NextRequest, context: RouteContext) {
  const { tenantId } = await context.params;
  return proxyAuthToGateway(
    request,
    `/api/auth/workspaces/${encodeURIComponent(tenantId)}`,
    "DELETE",
  );
}
