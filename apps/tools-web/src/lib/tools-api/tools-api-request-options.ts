import "server-only";

import { getToolsAccessToken, getRequestCorrelationId } from "@/lib/tools-auth/tools-auth-headers";

import type { ToolsApiRequestOptions } from "./tools-api-types";

export async function getToolsApiRequestOptions(): Promise<ToolsApiRequestOptions> {
  const accessToken = await getToolsAccessToken();
  if (!accessToken) {
    throw new Error("Authenticated tools context is required.");
  }

  const options: ToolsApiRequestOptions = {
    authContext: {
      bearerToken: accessToken,
    },
  };

  const correlationId = await getRequestCorrelationId();
  if (correlationId) {
    options.correlationId = correlationId;
  }

  return options;
}
