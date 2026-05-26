import type { NextRequest } from "next/server";

import { proxyToGateway } from "@/lib/api/gateway-proxy";

export async function GET(request: NextRequest) {
  return proxyToGateway(request, {
    endpoint: "/api/v1/account/preferences",
    method: "GET",
  });
}
