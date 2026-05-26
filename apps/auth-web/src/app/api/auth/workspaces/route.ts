import type { NextRequest } from "next/server";

import { proxyToGateway } from "@/lib/api/gateway-proxy";
import { authEndpoints } from "@/features/auth/api/auth-endpoints";

export async function GET(request: NextRequest) {
  return proxyToGateway(request, {
    endpoint: authEndpoints.workspaces,
    method: "GET",
  });
}
