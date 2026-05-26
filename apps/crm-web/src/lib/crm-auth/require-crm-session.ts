import "server-only";

import { validateCrmSession, type CrmSession } from "./crm-session";

export async function requireCrmSession(pathname = "/"): Promise<CrmSession> {
  return validateCrmSession(pathname);
}
