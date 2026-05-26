import "server-only";

import { redirect } from "next/navigation";

import { getCurrentAuthSession } from "./auth-session";
import { getRedirectAfterAuth } from "../utils/redirect-after-auth";

export async function requireAnonymousAuthPage(returnUrl?: string | null): Promise<void> {
  const session = await getCurrentAuthSession();

  if (session.authenticated) {
    redirect(getRedirectAfterAuth(returnUrl));
  }
}
