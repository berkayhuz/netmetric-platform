import "server-only";

import { redirect } from "next/navigation";

import { crmCapabilityAllows, type CrmCapability } from "./crm-capabilities";
import { requireCrmSession } from "./require-crm-session";
import type { CrmSession } from "./crm-session";

export async function requireCrmActionCapability(
  pathname: string,
  capability: CrmCapability,
): Promise<CrmSession> {
  const session = await requireCrmSession(pathname);
  if (!crmCapabilityAllows(session.capabilities, capability)) {
    redirect("/access-denied");
  }

  return session;
}
