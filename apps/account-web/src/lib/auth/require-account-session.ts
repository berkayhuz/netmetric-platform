import "server-only";

import { redirect } from "next/navigation";

import { AccountApiError } from "@/lib/account-api";

import { getCurrentAccountSession } from "./account-session";
import { buildAccountSessionResetUrl, buildAuthLoginRedirectUrl } from "./safe-return-url";

export async function requireAccountSession(pathname = "/"): Promise<void> {
  try {
    const session = await getCurrentAccountSession();

    if (!session.authenticated) {
      if (session.auth.unavailable) {
        redirect("/service-unavailable");
      }

      if (session.auth.status === 401) {
        redirect(buildAccountSessionResetUrl(pathname));
      }

      redirect(buildAuthLoginRedirectUrl(pathname));
    }
  } catch (error) {
    if (error instanceof AccountApiError) {
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
}
