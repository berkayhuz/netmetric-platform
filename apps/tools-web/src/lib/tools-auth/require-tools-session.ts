import "server-only";

import { redirect } from "next/navigation";

import { getCurrentToolsSession, getToolsAccessToken } from "./tools-auth-headers";
import { buildAuthLoginRedirectUrl } from "./safe-return-url";

export async function requireToolsSession(pathname = "/"): Promise<string> {
  const session = await getCurrentToolsSession();
  if (!session.isAuthenticated) {
    redirect(buildAuthLoginRedirectUrl(pathname));
  }

  const token = await getToolsAccessToken();
  if (!token) {
    redirect(buildAuthLoginRedirectUrl(pathname));
  }

  return token;
}
