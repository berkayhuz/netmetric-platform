import "server-only";

import { redirect } from "next/navigation";

import { AccountApiError } from "@/lib/account-api";

export function handleAccountApiPageError(error: unknown): never {
  if (error instanceof AccountApiError) {
    if (error.kind === "unauthorized") {
      redirect("/");
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
  }

  throw error;
}
