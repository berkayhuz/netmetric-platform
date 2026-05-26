import "server-only";

import { redirect } from "next/navigation";

import { ToolsApiError } from "@/lib/tools-api";

import { buildAuthLoginRedirectUrl } from "./safe-return-url";

export function handleToolsApiPageError(error: unknown, returnPath = "/history"): never {
  if (error instanceof ToolsApiError) {
    if (error.kind === "unauthorized") {
      redirect(buildAuthLoginRedirectUrl(returnPath));
    }

    if (error.kind === "forbidden") {
      redirect("/access-denied");
    }

    if (error.kind === "rate_limited") {
      redirect("/retry-later");
    }

    if (error.kind === "server_error" || error.kind === "upstream_unavailable") {
      redirect("/service-unavailable");
    }

    if (error.kind === "not_found") {
      redirect("/history");
    }
  }

  throw error;
}
