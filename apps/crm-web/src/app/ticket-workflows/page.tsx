import Link from "next/link";
import {
  CrmMetricGrid,
  CrmSectionCard,
  CrmToolbarSurface,
} from "@/components/shell/crm-content-primitives";
import { CrmEmptyState } from "@/components/shell/crm-empty-state";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CrmRecordsTable, type CrmRecordsTableRow } from "@/components/shell/crm-records-table";
import { isGuid } from "@/features/shared/data/guid";
import { getTicketWorkflowData } from "@/features/ticket-workflows/data/ticket-workflow-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDateTime } from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import type {
  TicketAssignmentHistoryDto,
  TicketStatusHistoryDto,
  TicketWorkflowQueueDto,
} from "@/lib/crm-api";
import { Badge, Button, Input } from "@netmetric/ui";
import { History, Inbox, Route, Shuffle, TicketCheck } from "lucide-react";

function toSingleValue(value: string | string[] | undefined): string | undefined {
  if (typeof value === "string") return value;
  if (Array.isArray(value)) return value[0];
  return undefined;
}

function enumLabel(namespace: string, value: string, locale: string): string {
  return tCrmWithFallback(`${namespace}.${value}`, value, locale);
}

export default async function TicketWorkflowsPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  await requireCrmSession("/ticket-workflows");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const params = await searchParams;
  const ticketIdRaw = toSingleValue(params.ticketId);
  const ticketId = ticketIdRaw && isGuid(ticketIdRaw) ? ticketIdRaw : undefined;

  const { queues, assignments, statusHistory } = await getTicketWorkflowData(
    {
      ...(ticketId ? { ticketId } : {}),
    },
    "/ticket-workflows",
  );
  const defaultQueue = queues.find((queue) => queue.isDefault);

  return (
    <CrmPageShell
      routePath="/ticket-workflows"
      locale={locale}
      actions={
        <div className="flex flex-wrap gap-2">
          <Button asChild variant="outline">
            <Link href="/ticket-workflows/queues">
              <Inbox aria-hidden="true" />
              {tCrm("crm.ticketWorkflows.queues.title", locale)}
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/ticket-workflows/assignment-history">
              <Shuffle aria-hidden="true" />
              {tCrm("crm.ticketWorkflows.assignmentHistory.title", locale)}
            </Link>
          </Button>
        </div>
      }
    >
      <CrmMetricGrid
        columns="four"
        items={[
          {
            label: tCrm("crm.ticketWorkflows.queues.title", locale),
            value: queues.length,
            description:
              defaultQueue?.name ?? tCrm("crm.ticketWorkflows.queues.description", locale),
            icon: <Inbox aria-hidden="true" className="size-4" />,
          },
          {
            label: tCrm("crm.ticketWorkflows.fields.ticketId", locale),
            value: ticketId
              ? tCrm("crm.modules.workspace.current", locale)
              : tCrm("crm.ticketWorkflows.selectTicket.title", locale),
            description: tCrm("crm.ticketWorkflows.readFilters.description", locale),
            icon: <TicketCheck aria-hidden="true" className="size-4" />,
            tone: ticketId ? "success" : "neutral",
          },
          {
            label: tCrm("crm.ticketWorkflows.assignmentHistory.title", locale),
            value: assignments?.length ?? 0,
            description: tCrm("crm.ticketWorkflows.assignmentHistory.description", locale),
            icon: <Shuffle aria-hidden="true" className="size-4" />,
          },
          {
            label: tCrm("crm.ticketWorkflows.statusHistory.title", locale),
            value: statusHistory?.length ?? 0,
            description: tCrm("crm.ticketWorkflows.statusHistory.description", locale),
            icon: <History aria-hidden="true" className="size-4" />,
          },
        ]}
      />

      <CrmToolbarSurface
        title={tCrm("crm.ticketWorkflows.readFilters.title", locale)}
        description={tCrm("crm.ticketWorkflows.readFilters.description", locale)}
      >
        <form method="get" className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_auto]">
          <div className="space-y-2">
            <label htmlFor="workflow-ticketId" className="text-sm font-medium">
              {tCrm("crm.ticketWorkflows.fields.ticketIdGuid", locale)}
            </label>
            <Input
              id="workflow-ticketId"
              name="ticketId"
              defaultValue={ticketId ?? ticketIdRaw ?? ""}
              className="h-9 rounded-md bg-background"
            />
          </div>
          <div className="flex items-end">
            <Button type="submit" variant="outline" className="w-full lg:w-auto">
              <Route aria-hidden="true" />
              {tCrm("crm.ticketWorkflows.readFilters.load", locale)}
            </Button>
          </div>
        </form>
        {ticketIdRaw && !ticketId ? (
          <p className="mt-3 text-sm text-muted-foreground">
            {tCrm("crm.ticketWorkflows.validation.invalidTicketId", locale)}
          </p>
        ) : null}
      </CrmToolbarSurface>

      <CrmSectionCard
        title={tCrm("crm.ticketWorkflows.queues.title", locale)}
        description={tCrm("crm.ticketWorkflows.queues.description", locale)}
        actions={<Badge variant="secondary">{queues.length}</Badge>}
        contentClassName={queues.length === 0 ? undefined : "p-0"}
      >
        {queues.length === 0 ? (
          <CrmEmptyState
            title={tCrm("crm.ticketWorkflows.queues.emptyTitle", locale)}
            description={tCrm("crm.ticketWorkflows.queues.emptyDescription", locale)}
            icon={<Inbox aria-hidden="true" />}
          />
        ) : (
          <QueuesTable locale={locale} queues={queues} />
        )}
      </CrmSectionCard>

      <div className="grid gap-4 xl:grid-cols-2">
        <CrmSectionCard
          title={tCrm("crm.ticketWorkflows.assignmentHistory.title", locale)}
          description={tCrm("crm.ticketWorkflows.assignmentHistory.description", locale)}
          actions={<Badge variant="outline">{assignments?.length ?? 0}</Badge>}
          contentClassName={
            !ticketId || !assignments || assignments.length === 0 ? undefined : "p-0"
          }
        >
          <AssignmentHistoryContent
            assignments={assignments}
            dateSettings={dateSettings}
            hasTicket={Boolean(ticketId)}
            locale={locale}
          />
        </CrmSectionCard>

        <CrmSectionCard
          title={tCrm("crm.ticketWorkflows.statusHistory.title", locale)}
          description={tCrm("crm.ticketWorkflows.statusHistory.description", locale)}
          actions={<Badge variant="outline">{statusHistory?.length ?? 0}</Badge>}
          contentClassName={
            !ticketId || !statusHistory || statusHistory.length === 0 ? undefined : "p-0"
          }
        >
          <StatusHistoryContent
            dateSettings={dateSettings}
            hasTicket={Boolean(ticketId)}
            locale={locale}
            statusHistory={statusHistory}
          />
        </CrmSectionCard>
      </div>
    </CrmPageShell>
  );
}

function QueuesTable({
  locale,
  queues,
}: Readonly<{ locale: string; queues: TicketWorkflowQueueDto[] }>) {
  const rows: CrmRecordsTableRow[] = queues.map((queue) => ({
    id: queue.id,
    href: `/ticket-workflows/queues/${queue.id}`,
    cells: {
      code: queue.code,
      name: queue.name,
      strategy: enumLabel("crm.ticketWorkflows.strategy", String(queue.assignmentStrategy), locale),
      isDefault: queue.isDefault
        ? tCrm("crm.common.boolean.true", locale)
        : tCrm("crm.common.boolean.false", locale),
    },
    descriptions: { name: queue.description ?? "-" },
  }));

  return (
    <CrmRecordsTable
      caption={tCrm("crm.ticketWorkflows.queues.caption", locale)}
      columns={[
        { key: "code", header: tCrm("crm.ticketWorkflows.fields.code", locale), sortable: true },
        { key: "name", header: tCrm("crm.ticketWorkflows.fields.name", locale), sortable: true },
        {
          key: "strategy",
          header: tCrm("crm.ticketWorkflows.fields.assignmentStrategy", locale),
          sortable: true,
        },
        {
          key: "isDefault",
          header: tCrm("crm.ticketWorkflows.fields.default", locale),
          sortable: true,
          badge: true,
        },
      ]}
      rows={rows}
      labels={{
        searchPlaceholder: tCrm("crm.common.search", locale),
        emptyTitle: tCrm("crm.ticketWorkflows.queues.emptyTitle", locale),
        emptyDescription: tCrm("crm.ticketWorkflows.queues.emptyDescription", locale),
      }}
    />
  );
}

function AssignmentHistoryContent({
  assignments,
  dateSettings,
  hasTicket,
  locale,
}: Readonly<{
  assignments: TicketAssignmentHistoryDto[] | null;
  dateSettings: Awaited<ReturnType<typeof getRequestDateSettings>>;
  hasTicket: boolean;
  locale: string;
}>) {
  if (!hasTicket) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketWorkflows.selectTicket.title", locale)}
        description={tCrm("crm.ticketWorkflows.selectTicket.description", locale)}
        icon={<Shuffle aria-hidden="true" />}
      />
    );
  }

  if (!assignments || assignments.length === 0) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketWorkflows.assignmentHistory.emptyTitle", locale)}
        description={tCrm("crm.ticketWorkflows.assignmentHistory.emptyDescription", locale)}
        icon={<Shuffle aria-hidden="true" />}
      />
    );
  }

  const rows: CrmRecordsTableRow[] = assignments.map((assignment) => ({
    id: assignment.id,
    cells: {
      changedAt: formatCrmDateTime(assignment.changedAtUtc, dateSettings),
      newQueue: assignment.newQueueId ?? "-",
      newOwner: assignment.newOwnerUserId ?? "-",
      reason: assignment.reason ?? "-",
    },
    descriptions: {
      newQueue: assignment.previousQueueId ?? "-",
      newOwner: assignment.previousOwnerUserId ?? "-",
    },
  }));

  return (
    <CrmRecordsTable
      caption={tCrm("crm.ticketWorkflows.assignmentHistory.caption", locale)}
      columns={[
        {
          key: "changedAt",
          header: tCrm("crm.ticketWorkflows.fields.changedAt", locale),
          sortable: true,
        },
        { key: "newQueue", header: tCrm("crm.ticketWorkflows.fields.newQueue", locale) },
        { key: "newOwner", header: tCrm("crm.ticketWorkflows.fields.newOwner", locale) },
        { key: "reason", header: tCrm("crm.ticketWorkflows.fields.reason", locale) },
      ]}
      rows={rows}
      labels={{
        searchPlaceholder: tCrm("crm.common.search", locale),
        emptyTitle: tCrm("crm.ticketWorkflows.assignmentHistory.emptyTitle", locale),
        emptyDescription: tCrm("crm.ticketWorkflows.assignmentHistory.emptyDescription", locale),
      }}
    />
  );
}

function StatusHistoryContent({
  dateSettings,
  hasTicket,
  locale,
  statusHistory,
}: Readonly<{
  dateSettings: Awaited<ReturnType<typeof getRequestDateSettings>>;
  hasTicket: boolean;
  locale: string;
  statusHistory: TicketStatusHistoryDto[] | null;
}>) {
  if (!hasTicket) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketWorkflows.selectTicket.title", locale)}
        description={tCrm("crm.ticketWorkflows.selectTicket.description", locale)}
        icon={<History aria-hidden="true" />}
      />
    );
  }

  if (!statusHistory || statusHistory.length === 0) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketWorkflows.statusHistory.emptyTitle", locale)}
        description={tCrm("crm.ticketWorkflows.statusHistory.emptyDescription", locale)}
        icon={<History aria-hidden="true" />}
      />
    );
  }

  const rows: CrmRecordsTableRow[] = statusHistory.map((status) => ({
    id: status.id,
    cells: {
      changedAt: formatCrmDateTime(status.changedAtUtc, dateSettings),
      newStatus: status.newStatus ?? "-",
      note: status.note ?? "-",
    },
    descriptions: { newStatus: status.previousStatus ?? "-" },
  }));

  return (
    <CrmRecordsTable
      caption={tCrm("crm.ticketWorkflows.statusHistory.caption", locale)}
      columns={[
        {
          key: "changedAt",
          header: tCrm("crm.ticketWorkflows.fields.changedAt", locale),
          sortable: true,
        },
        { key: "newStatus", header: tCrm("crm.ticketWorkflows.fields.newStatus", locale) },
        { key: "note", header: tCrm("crm.ticketWorkflows.fields.note", locale) },
      ]}
      rows={rows}
      labels={{
        searchPlaceholder: tCrm("crm.common.search", locale),
        emptyTitle: tCrm("crm.ticketWorkflows.statusHistory.emptyTitle", locale),
        emptyDescription: tCrm("crm.ticketWorkflows.statusHistory.emptyDescription", locale),
      }}
    />
  );
}
