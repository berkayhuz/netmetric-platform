import { notFound } from "next/navigation";
import Link from "next/link";
import { Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { getTicketSlaData } from "@/features/ticket-sla/data/ticket-sla-data";
import { getTicketWorkflowData } from "@/features/ticket-workflows/data/ticket-workflow-data";
import { TicketDetailWorkspace } from "@/features/tickets/components/ticket-detail-workspace";
import {
  getTicketActivitiesTimelineData,
  getTicketDetailData,
} from "@/features/tickets/data/tickets-data";
import { deleteTicketAction } from "@/features/tickets/actions/ticket-mutation-actions";
import { isGuid } from "@/features/shared/data/guid";
import {
  CrmApiError,
  type ActivityTimelineFeed,
  type TicketAssignmentHistoryDto,
  type TicketDetailDto,
  type TicketEscalationRunDto,
  type TicketSlaPolicyDto,
  type TicketSlaWorkspaceDto,
  type TicketStatusHistoryDto,
  type TicketWorkflowQueueDto,
} from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import { tCrm } from "@/lib/i18n/crm-i18n";

export default async function TicketDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  const session = await requireCrmSession(`/tickets/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "tickets.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "tickets.delete");
  const canReadWorkflow = crmCapabilityAllows(session.capabilities, "ticketWorkflow.read");
  const canManageWorkflow = crmCapabilityAllows(session.capabilities, "ticketWorkflow.manage");
  const canReadSla = crmCapabilityAllows(session.capabilities, "ticketSla.read");
  const canManageSla = crmCapabilityAllows(session.capabilities, "ticketSla.manage");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let ticket: TicketDetailDto;
  let queues: TicketWorkflowQueueDto[] = [];
  let assignments: TicketAssignmentHistoryDto[] | null = null;
  let statusHistory: TicketStatusHistoryDto[] | null = null;
  let slaPolicies: TicketSlaPolicyDto[] = [];
  let slaWorkspace: TicketSlaWorkspaceDto | null = null;
  let escalationRuns: TicketEscalationRunDto[] | null = null;
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;

  try {
    ticket = await getTicketDetailData(resolved.id, `/tickets/${resolved.id}`);
    if (canReadWorkflow) {
      const workflowData = await getTicketWorkflowData(
        { ticketId: resolved.id },
        `/tickets/${resolved.id}`,
      );
      queues = workflowData.queues;
      assignments = workflowData.assignments;
      statusHistory = workflowData.statusHistory;
    }

    if (canReadSla) {
      const slaQuery = {
        ...(ticket.slaPolicyId ? { policyId: ticket.slaPolicyId } : {}),
        ...(ticket.slaPolicyId ? { ticketId: resolved.id } : {}),
      };
      const slaData = await getTicketSlaData(slaQuery, `/tickets/${resolved.id}`);
      slaPolicies = slaData.policies;
      slaWorkspace = slaData.workspace;
      escalationRuns = slaData.escalationRuns;
    }
    if (canReadActivities) {
      try {
        unifiedTimelineFeed = await getTicketActivitiesTimelineData(
          resolved.id,
          `/tickets/${resolved.id}`,
          {
            page: 1,
            pageSize: 10,
          },
        );
      } catch {
        isUnifiedTimelineUnavailable = true;
      }
    }
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/tickets/${resolved.id}`);
  }

  return (
    <CrmPageShell
      title={ticket.subject}
      description={tCrm("crm.tickets.detail.description", locale)}
      actions={
        canEdit ? (
          <Button asChild>
            <Link href={`/tickets/${resolved.id}/edit`}>
              {tCrm("crm.tickets.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <TicketDetailWorkspace
        ticket={ticket}
        queues={queues}
        assignments={assignments}
        statusHistory={statusHistory}
        slaPolicies={slaPolicies}
        slaWorkspace={slaWorkspace}
        escalationRuns={escalationRuns}
        canManageWorkflow={canManageWorkflow}
        canManageSla={canManageSla}
        canReadActivities={canReadActivities}
        canCreateActivities={canCreateActivities}
        unifiedTimeline={unifiedTimelineFeed}
        isUnifiedTimelineUnavailable={isUnifiedTimelineUnavailable}
        dateSettings={dateSettings}
        locale={locale}
      />
      {canDelete ? (
        <CrmDeleteZone
          title={tCrm("crm.tickets.delete.title", locale)}
          description={tCrm("crm.tickets.delete.description", locale)}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.tickets.entityLabel", locale)}
            entityName={ticket.subject}
            confirmValue="delete-ticket"
            action={deleteTicketAction.bind(null, resolved.id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
