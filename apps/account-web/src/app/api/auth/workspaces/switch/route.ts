import type { NextRequest } from "next/server";

import { proxyAuthToGateway } from "@/lib/auth/auth-gateway-proxy";

export async function POST(request: NextRequest) {
  return proxyAuthToGateway(request, "/api/auth/workspaces/switch", "POST");
}
