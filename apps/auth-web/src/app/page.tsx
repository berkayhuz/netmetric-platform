import { authRoutes } from "@/features/auth/config/auth-routes";
import { getRequestLocale } from "@/features/auth/i18n/auth-i18n.server";
import { createAuthMetadata } from "@/features/auth/i18n/auth-metadata";
import { getCurrentAuthSession } from "@/features/auth/server/auth-session";
import { getRedirectAfterAuth } from "@/features/auth/utils/redirect-after-auth";

import { redirect } from "next/navigation";

export async function generateMetadata() {
  return createAuthMetadata(await getRequestLocale(), "auth.login.metaTitle");
}

export default async function HomePage() {
  const session = await getCurrentAuthSession();
  if (session.authenticated) {
    redirect(getRedirectAfterAuth());
  }

  redirect(authRoutes.login);
}
