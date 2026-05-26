import "server-only";

import { isGuid } from "@/features/shared/data/guid";
import {
  crmApiClient,
  type TicketEscalationRunDto,
  type TicketSlaEscalationRuleDto,
  type TicketSlaPolicyDto,
  type TicketSlaWorkspaceDto,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";

export type TicketSlaReadQuery = {
  policyId?: string;
  ticketId?: string;
};

export type TicketSlaDataResult = {
  policies: TicketSlaPolicyDto[];
  escalationRules: TicketSlaEscalationRuleDto[] | null;
  workspace: TicketSlaWorkspaceDto | null;
  escalationRuns: TicketEscalationRunDto[] | null;
};

export async function getTicketSlaData(
  query: TicketSlaReadQuery,
  returnPath: string,
): Promise<TicketSlaDataResult> {
  await requireCrmSession("/");

  try {
    const options = await getCrmApiRequestOptions();

    const policies = await crmApiClient.listTicketSlaPolicies(options);
    const escalationRules =
      query.policyId && isGuid(query.policyId)
        ? await crmApiClient.listTicketSlaEscalationRules(query.policyId, options)
        : null;

    const [workspace, escalationRuns] =
      query.ticketId && isGuid(query.ticketId)
        ? await Promise.all([
            crmApiClient.getTicketSlaWorkspace(query.ticketId, options),
            crmApiClient.listTicketEscalationRuns(query.ticketId, options),
          ])
        : [null, null];

    return {
      policies,
      escalationRules,
      workspace,
      escalationRuns,
    };
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
