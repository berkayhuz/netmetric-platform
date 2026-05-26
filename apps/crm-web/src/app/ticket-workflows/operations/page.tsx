import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import {
  assignTicketWorkflowOwnerAction,
  assignTicketWorkflowQueueAction,
  changeTicketWorkflowStatusAction,
  createTicketWorkflowQueueAction,
  deleteTicketWorkflowQueueAction,
  updateTicketWorkflowQueueAction,
} from "@/features/ticket-workflows/actions/ticket-workflow-mutation-actions";
import { TicketWorkflowMutationPanels } from "@/features/ticket-workflows/forms/ticket-workflow-mutation-panels";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function TicketWorkflowOperationsPage() {
  await requireCrmSession("/ticket-workflows/operations");
  const locale = await getRequestLocale();

  return (
    <CrmPageShell
      title={tCrm("crm.ticketWorkflows.mutations.title", locale)}
      description={tCrm("crm.ticketWorkflows.mutations.description", locale)}
      locale={locale}
      actions={
        <Button asChild variant="outline">
          <Link href="/ticket-workflows">
            <ArrowLeft aria-hidden="true" />
            {tCrm("crm.common.backToList", locale)}
          </Link>
        </Button>
      }
    >
      <TicketWorkflowMutationPanels
        createQueueAction={createTicketWorkflowQueueAction}
        updateQueueAction={updateTicketWorkflowQueueAction}
        deleteQueueAction={deleteTicketWorkflowQueueAction}
        assignQueueAction={assignTicketWorkflowQueueAction}
        assignOwnerAction={assignTicketWorkflowOwnerAction}
        statusChangeAction={changeTicketWorkflowStatusAction}
      />
    </CrmPageShell>
  );
}
