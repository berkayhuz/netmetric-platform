import "server-only";

import type { CrmApiRequestOptions } from "@/lib/crm-api";
import { getCrmApiAuthContext, getRequestCorrelationId } from "@/lib/crm-auth/crm-auth-headers";

export async function getCrmApiRequestOptions(): Promise<CrmApiRequestOptions> {
  const authContext = await getCrmApiAuthContext();
  const correlationId = await getRequestCorrelationId();

  return {
    ...(authContext ? { authContext } : {}),
    ...(correlationId ? { correlationId } : {}),
  };
}
