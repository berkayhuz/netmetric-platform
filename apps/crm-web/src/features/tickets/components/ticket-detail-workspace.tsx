import {
  Badge,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
} from "@netmetric/ui";

import { ActivityTimelinePanel } from "@/features/activities/components/activity-timeline-panel";
import { ActivityComposer } from "@/features/activities/components/activity-composer";
import { TicketDetailActionPanels } from "@/features/tickets/components/ticket-detail-action-panels";
import type {
  ActivityTimelineFeed,
  TicketAssignmentHistoryDto,
  TicketDetailDto,
  TicketEscalationRunDto,
  TicketSlaPolicyDto,
  TicketSlaWorkspaceDto,
  TicketStatusHistoryDto,
  TicketWorkflowQueueDto,
} from "@/lib/crm-api";
import { type CrmDateSettings, formatCrmDateTime } from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";

type TicketDetailWorkspaceProps = {
  ticket: TicketDetailDto;
  queues: TicketWorkflowQueueDto[];
  assignments: TicketAssignmentHistoryDto[] | null;
  statusHistory: TicketStatusHistoryDto[] | null;
  slaPolicies: TicketSlaPolicyDto[];
  slaWorkspace: TicketSlaWorkspaceDto | null;
  escalationRuns: TicketEscalationRunDto[] | null;
  canManageWorkflow: boolean;
  canManageSla: boolean;
  canReadActivities: boolean;
  canCreateActivities: boolean;
  unifiedTimeline: ActivityTimelineFeed;
  isUnifiedTimelineUnavailable: boolean;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
};

export function TicketDetailWorkspace({
  ticket,
  queues,
  assignments,
  statusHistory,
  slaPolicies,
  slaWorkspace,
  escalationRuns,
  canManageWorkflow,
  canManageSla,
  canReadActivities,
  canCreateActivities,
  unifiedTimeline,
  isUnifiedTimelineUnavailable,
  dateSettings,
  locale,
}: Readonly<TicketDetailWorkspaceProps>) {
  const currentQueueId = assignments?.[0]?.newQueueId ?? null;

  return (
    <section className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <TicketMetricCard
          title={tCrm("crm.tickets.fields.status", locale)}
          value={formatTicketStatus(ticket.status, locale)}
        />
        <TicketMetricCard
          title={tCrm("crm.tickets.fields.priority", locale)}
          value={formatPriority(ticket.priority, locale)}
        />
        <TicketMetricCard
          title={tCrm("crm.tickets.fields.openedAt", locale)}
          value={formatCrmDateTime(ticket.openedAt, dateSettings)}
        />
        <TicketMetricCard
          title={tCrm("crm.tickets.sla.breachState", locale)}
          value={
            slaWorkspace?.isResolutionBreached || slaWorkspace?.isFirstResponseBreached
              ? tCrm("crm.common.yes", locale)
              : tCrm("crm.common.no", locale)
          }
        />
      </div>

      <TicketProfileCard ticket={ticket} dateSettings={dateSettings} locale={locale} />

      <div className="grid gap-4 xl:grid-cols-2">
        <TicketCommentsCard ticket={ticket} dateSettings={dateSettings} locale={locale} />
        <TicketWorkflowCard
          assignments={assignments}
          queues={queues}
          statusHistory={statusHistory}
          dateSettings={dateSettings}
          locale={locale}
        />
      </div>

      <TicketSlaCard
        escalationRuns={escalationRuns}
        policies={slaPolicies}
        workspace={slaWorkspace}
        dateSettings={dateSettings}
        locale={locale}
      />
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "ticket", entityId: ticket.id }}
              locale={locale}
            />
          ) : null}
          <ActivityTimelinePanel
            activities={unifiedTimeline.items}
            dateSettings={dateSettings}
            locale={locale ?? "en"}
            title={tCrm("crm.activities.sections.unifiedTimelinePreviewTitle", locale)}
            description={tCrm("crm.activities.sections.unifiedTimelinePreviewDescription", locale)}
            unavailable={isUnifiedTimelineUnavailable}
          />
        </div>
      ) : null}

      {canManageWorkflow || canManageSla ? (
        <Card>
          <CardHeader>
            <CardTitle>{tCrm("crm.tickets.actions.title", locale)}</CardTitle>
            <CardDescription>{tCrm("crm.tickets.actions.description", locale)}</CardDescription>
          </CardHeader>
          <CardContent>
            <TicketDetailActionPanels
              ticket={ticket}
              queues={queues}
              policies={slaPolicies}
              currentQueueId={currentQueueId}
              canManageWorkflow={canManageWorkflow}
              canManageSla={canManageSla}
            />
          </CardContent>
        </Card>
      ) : null}
    </section>
  );
}

function TicketMetricCard({
  title,
  value,
}: Readonly<{ title: string; value: string | number | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardDescription>{title}</CardDescription>
        <CardTitle className="text-3xl">{value ?? "-"}</CardTitle>
      </CardHeader>
    </Card>
  );
}

function TicketProfileCard({
  ticket,
  dateSettings,
  locale,
}: Readonly<{
  ticket: TicketDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  const fields = [
    { label: tCrm("crm.tickets.fields.ticketNumber", locale), value: ticket.ticketNumber },
    { label: tCrm("crm.tickets.fields.subject", locale), value: ticket.subject },
    {
      label: tCrm("crm.tickets.fields.status", locale),
      value: formatTicketStatus(ticket.status, locale),
    },
    {
      label: tCrm("crm.tickets.fields.priority", locale),
      value: formatPriority(ticket.priority, locale),
    },
    {
      label: tCrm("crm.tickets.fields.type", locale),
      value: formatTicketType(ticket.ticketType, locale),
    },
    {
      label: tCrm("crm.tickets.fields.channel", locale),
      value: formatTicketChannel(ticket.channel, locale),
    },
    { label: tCrm("crm.tickets.fields.assignedUserId", locale), value: ticket.assignedUserId },
    { label: tCrm("crm.tickets.fields.customerId", locale), value: ticket.customerId },
    { label: tCrm("crm.tickets.fields.contactId", locale), value: ticket.contactId },
    { label: tCrm("crm.tickets.fields.ticketCategoryId", locale), value: ticket.ticketCategoryId },
    { label: tCrm("crm.ticketSla.fields.policyId", locale), value: ticket.slaPolicyId },
    {
      label: tCrm("crm.tickets.fields.firstResponseDueAt", locale),
      value: formatCrmDateTime(ticket.firstResponseDueAt, dateSettings),
    },
    {
      label: tCrm("crm.tickets.fields.resolveDueAt", locale),
      value: formatCrmDateTime(ticket.resolveDueAt, dateSettings),
    },
    {
      label: tCrm("crm.tickets.fields.openedAt", locale),
      value: formatCrmDateTime(ticket.openedAt, dateSettings),
    },
    {
      label: tCrm("crm.tickets.fields.closedAt", locale),
      value: formatCrmDateTime(ticket.closedAt, dateSettings),
    },
    {
      label: tCrm("crm.tickets.fields.state", locale),
      value: ticket.isActive
        ? tCrm("crm.common.active", locale)
        : tCrm("crm.common.inactive", locale),
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.tickets.detail.profileTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.tickets.detail.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <dl className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {fields.map((field) => (
            <div key={field.label} className="space-y-1">
              <dt className="text-xs font-medium uppercase text-muted-foreground">{field.label}</dt>
              <dd className="break-words text-sm">{field.value ?? "-"}</dd>
            </div>
          ))}
        </dl>
        <div className="grid gap-4 lg:grid-cols-2">
          <LongTextBlock
            title={tCrm("crm.tickets.fields.description", locale)}
            value={ticket.description}
          />
          <LongTextBlock title={tCrm("crm.tickets.fields.notes", locale)} value={ticket.notes} />
        </div>
      </CardContent>
    </Card>
  );
}

function TicketCommentsCard({
  ticket,
  dateSettings,
  locale,
}: Readonly<{
  ticket: TicketDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.tickets.comments.title", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.tickets.comments.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {ticket.comments.length > 0 ? (
          ticket.comments.map((comment) => (
            <div key={comment.id} className="rounded border border-border p-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <p className="text-xs text-muted-foreground">
                  {formatCrmDateTime(comment.createdAt, dateSettings)}
                </p>
                <Badge variant="outline">
                  {comment.isInternal
                    ? tCrm("crm.tickets.comments.internal", locale)
                    : tCrm("crm.tickets.comments.public", locale)}
                </Badge>
              </div>
              <p className="mt-3 whitespace-pre-wrap text-sm">{comment.comment}</p>
              <p className="mt-2 font-mono text-xs text-muted-foreground">
                {comment.createdBy ?? "-"}
              </p>
            </div>
          ))
        ) : (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.tickets.comments.empty", locale)}
          </Text>
        )}
      </CardContent>
    </Card>
  );
}

function TicketWorkflowCard({
  assignments,
  queues,
  statusHistory,
  dateSettings,
  locale,
}: Readonly<{
  assignments: TicketAssignmentHistoryDto[] | null;
  queues: TicketWorkflowQueueDto[];
  statusHistory: TicketStatusHistoryDto[] | null;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.tickets.workflow.title", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.tickets.workflow.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <div className="grid gap-3 sm:grid-cols-2">
          <StatBox title={tCrm("crm.ticketWorkflows.queues.title", locale)} value={queues.length} />
          <StatBox
            title={tCrm("crm.ticketWorkflows.assignmentHistory.title", locale)}
            value={assignments?.length ?? 0}
          />
        </div>
        <HistoryTable
          emptyLabel={tCrm("crm.tickets.workflow.noAssignments", locale)}
          rows={(assignments ?? []).map((item) => ({
            id: item.id,
            primary: item.newOwnerUserId ?? item.newQueueId ?? "-",
            secondary: item.reason ?? "-",
            at: formatCrmDateTime(item.changedAtUtc, dateSettings),
          }))}
          title={tCrm("crm.ticketWorkflows.assignmentHistory.title", locale)}
        />
        <HistoryTable
          emptyLabel={tCrm("crm.tickets.workflow.noStatusHistory", locale)}
          rows={(statusHistory ?? []).map((item) => ({
            id: item.id,
            primary: `${item.previousStatus} -> ${item.newStatus}`,
            secondary: item.note ?? "-",
            at: formatCrmDateTime(item.changedAtUtc, dateSettings),
          }))}
          title={tCrm("crm.ticketWorkflows.statusHistory.title", locale)}
        />
      </CardContent>
    </Card>
  );
}

function TicketSlaCard({
  escalationRuns,
  policies,
  workspace,
  dateSettings,
  locale,
}: Readonly<{
  escalationRuns: TicketEscalationRunDto[] | null;
  policies: TicketSlaPolicyDto[];
  workspace: TicketSlaWorkspaceDto | null;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.tickets.sla.title", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.tickets.sla.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
          <StatBox title={tCrm("crm.ticketSla.policies.title", locale)} value={policies.length} />
          <StatBox
            title={tCrm("crm.ticketSla.fields.firstResponseDue", locale)}
            value={formatCrmDateTime(workspace?.firstResponseDueAtUtc, dateSettings)}
          />
          <StatBox
            title={tCrm("crm.ticketSla.fields.resolutionDue", locale)}
            value={formatCrmDateTime(workspace?.resolutionDueAtUtc, dateSettings)}
          />
          <StatBox
            title={tCrm("crm.tickets.sla.escalationRuns", locale)}
            value={escalationRuns?.length ?? 0}
          />
        </div>
        {workspace ? (
          <dl className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <DetailField
              label={tCrm("crm.ticketSla.fields.firstRespondedAt", locale)}
              value={formatCrmDateTime(workspace.firstRespondedAtUtc, dateSettings)}
            />
            <DetailField
              label={tCrm("crm.ticketSla.fields.resolvedAt", locale)}
              value={formatCrmDateTime(workspace.resolvedAtUtc, dateSettings)}
            />
            <DetailField
              label={tCrm("crm.ticketSla.fields.firstResponseBreached", locale)}
              value={formatBoolean(workspace.isFirstResponseBreached, locale)}
            />
            <DetailField
              label={tCrm("crm.ticketSla.fields.resolutionBreached", locale)}
              value={formatBoolean(workspace.isResolutionBreached, locale)}
            />
          </dl>
        ) : (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.tickets.sla.noWorkspace", locale)}
          </Text>
        )}
        <HistoryTable
          emptyLabel={tCrm("crm.tickets.sla.noEscalationRuns", locale)}
          rows={(escalationRuns ?? []).map((item) => ({
            id: item.id,
            primary: item.metricType,
            secondary: item.note,
            at: formatCrmDateTime(item.executedAtUtc, dateSettings),
          }))}
          title={tCrm("crm.tickets.sla.escalationRuns", locale)}
        />
      </CardContent>
    </Card>
  );
}

function HistoryTable({
  emptyLabel,
  rows,
  title,
}: Readonly<{
  emptyLabel: string;
  rows: Array<{ id: string; primary: string; secondary: string; at: string }>;
  title: string;
}>) {
  return (
    <div className="space-y-3">
      <h3 className="text-sm font-medium">{title}</h3>
      {rows.length > 0 ? (
        <div className="overflow-x-auto rounded border border-border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{title}</TableHead>
                <TableHead>{tCrm("crm.common.note")}</TableHead>
                <TableHead>{tCrm("crm.common.date")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {rows.map((row) => (
                <TableRow key={row.id}>
                  <TableCell>{row.primary}</TableCell>
                  <TableCell>{row.secondary}</TableCell>
                  <TableCell>{row.at}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      ) : (
        <Text className="text-sm text-muted-foreground">{emptyLabel}</Text>
      )}
    </div>
  );
}

function LongTextBlock({
  title,
  value,
}: Readonly<{ title: string; value?: string | null | undefined }>) {
  return (
    <div className="space-y-2">
      <h3 className="text-sm font-medium">{title}</h3>
      <Text className="whitespace-pre-wrap text-sm text-muted-foreground">{value ?? "-"}</Text>
    </div>
  );
}

function DetailField({ label, value }: Readonly<{ label: string; value: string | number }>) {
  return (
    <div className="space-y-1">
      <dt className="text-xs font-medium uppercase text-muted-foreground">{label}</dt>
      <dd className="break-words text-sm">{value}</dd>
    </div>
  );
}

function StatBox({ title, value }: Readonly<{ title: string; value: string | number }>) {
  return (
    <div className="rounded border border-border p-4">
      <p className="text-sm text-muted-foreground">{title}</p>
      <p className="mt-2 text-2xl font-semibold">{value}</p>
    </div>
  );
}

function formatTicketStatus(status: string | number, locale?: string | null | undefined) {
  return tCrmWithFallback(`crm.tickets.status.${status}`, String(status), locale);
}

function formatPriority(priority: string | number, locale?: string | null | undefined) {
  return tCrmWithFallback(`crm.common.priority.${priority}`, String(priority), locale);
}

function formatTicketType(type: string | number, locale?: string | null | undefined) {
  return tCrmWithFallback(`crm.tickets.type.${type}`, String(type), locale);
}

function formatTicketChannel(channel: string | number, locale?: string | null | undefined) {
  return tCrmWithFallback(`crm.tickets.channel.${channel}`, String(channel), locale);
}

function formatBoolean(value: boolean, locale?: string | null | undefined): string {
  return value ? tCrm("crm.common.yes", locale) : tCrm("crm.common.no", locale);
}
