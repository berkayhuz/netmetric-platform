import "server-only";

import { notFound, redirect } from "next/navigation";

import { CrmApiError } from "@/lib/crm-api";
import { buildCrmSessionResetUrl } from "@/lib/crm-auth/safe-return-url";

export function handleCrmApiPageError(error: unknown, returnPath = "/"): never {
  if (error instanceof CrmApiError) {
    if (error.kind === "unauthorized") {
      redirect(buildCrmSessionResetUrl(returnPath));
    }

    if (error.kind === "forbidden") {
      redirect("/access-denied");
    }

    if (error.kind === "rate_limited") {
      redirect("/retry-later");
    }

    if (error.kind === "not_found") {
      notFound();
    }

    if (error.kind === "server_error" || error.kind === "upstream_unavailable") {
      redirect("/service-unavailable");
    }
  }

  throw error;
}
