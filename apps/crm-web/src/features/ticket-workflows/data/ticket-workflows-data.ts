import "server-only";

import { isGuid } from "@/features/shared/data/guid";
import {
  crmApiClient,
  type TicketAssignmentHistoryDto,
  type TicketStatusHistoryDto,
  type TicketWorkflowQueueDto,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";

export type TicketWorkflowsReadQuery = {
  ticketId?: string;
};

export type TicketWorkflowsDataResult = {
  queues: TicketWorkflowQueueDto[];
  assignmentHistory: TicketAssignmentHistoryDto[] | null;
  statusHistory: TicketStatusHistoryDto[] | null;
};

export async function getTicketWorkflowsData(
  query: TicketWorkflowsReadQuery,
  returnPath: string,
): Promise<TicketWorkflowsDataResult> {
  await requireCrmSession("/");

  try {
    const options = await getCrmApiRequestOptions();

    const queues = await crmApiClient.listTicketWorkflowQueues(options);

    const [assignmentHistory, statusHistory] =
      query.ticketId && isGuid(query.ticketId)
        ? await Promise.all([
            crmApiClient.listTicketAssignmentHistory(query.ticketId, options),
            crmApiClient.listTicketStatusHistory(query.ticketId, options),
          ])
        : [null, null];

    return { queues, assignmentHistory, statusHistory };
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
