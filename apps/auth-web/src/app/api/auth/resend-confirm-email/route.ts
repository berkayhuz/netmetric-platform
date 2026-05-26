import type { NextRequest } from "next/server";

import { proxyToGateway } from "@/lib/api/gateway-proxy";
import { authEndpoints } from "@/features/auth/api/auth-endpoints";

export async function POST(request: NextRequest) {
  return proxyToGateway(request, {
    endpoint: authEndpoints.resendConfirmEmail,
    method: "POST",
  });
}
