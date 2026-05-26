import Link from "next/link";
import {
  CrmMetricGrid,
  CrmSectionCard,
  CrmToolbarSurface,
} from "@/components/shell/crm-content-primitives";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CrmPagination } from "@/components/shell/crm-pagination";
import { CrmRecordsTable, type CrmRecordsTableRow } from "@/components/shell/crm-records-table";
import { SupportInboxFilterForm } from "@/features/support-inbox/components/support-inbox-filter-form";
import { getSupportInboxData } from "@/features/support-inbox/data/support-inbox-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDateTime } from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import type { SupportInboxConnectionDto, SupportInboxMessageDto } from "@/lib/crm-api";
import { Badge, Button } from "@netmetric/ui";
import { Inbox, Link2, MailCheck, PlugZap, Route } from "lucide-react";

function toSingleValue(value: string | string[] | undefined): string | undefined {
  if (typeof value === "string") {
    return value;
  }

  if (Array.isArray(value)) {
    return value[0];
  }

  return undefined;
}

function toPositiveInt(value: string | undefined, fallback: number): number {
  if (!value) {
    return fallback;
  }

  const parsed = Number.parseInt(value, 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
}

export default async function SupportInboxPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  await requireCrmSession("/support-inbox");
  const params = await searchParams;
  const connectionId = toSingleValue(params.connectionId);
  const linkedToTicketValue = toSingleValue(params.linkedToTicket);
  const linkedToTicket =
    linkedToTicketValue === "true" ? true : linkedToTicketValue === "false" ? false : undefined;
  const page = toPositiveInt(toSingleValue(params.page), 1);
  const pageSize = toPositiveInt(toSingleValue(params.pageSize), 20);

  const { connections, messages } = await getSupportInboxData(
    {
      ...(connectionId ? { connectionId } : {}),
      ...(linkedToTicket !== undefined ? { linkedToTicket } : {}),
      page,
      pageSize,
    },
    "/support-inbox",
  );

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }
  const activeConnectionCount = connections.filter((connection) => connection.isActive).length;
  const selectedConnection = connectionId
    ? connections.find((connection) => connection.id === connectionId)
    : undefined;
  const linkedMessagesOnPage = messages.items.filter((message) => Boolean(message.ticketId)).length;

  return (
    <CrmPageShell
      routePath="/support-inbox"
      locale={locale}
      actions={
        <div className="flex flex-wrap gap-2">
          <Button asChild variant="outline">
            <Link href="/support-inbox/connections">
              <PlugZap aria-hidden="true" />
              {tCrm("crm.supportInbox.connections.title", locale)}
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/support-inbox/messages">
              <Route aria-hidden="true" />
              {tCrm("crm.supportInbox.messages.title", locale)}
            </Link>
          </Button>
        </div>
      }
    >
      <CrmMetricGrid
        columns="four"
        items={[
          {
            label: tCrm("crm.supportInbox.connections.title", locale),
            value: connections.length,
            description: tCrm("crm.supportInbox.connections.description", locale),
            icon: <PlugZap aria-hidden="true" className="size-4" />,
          },
          {
            label: tCrm("crm.supportInbox.state.active", locale),
            value: activeConnectionCount,
            description:
              selectedConnection?.emailAddress ??
              tCrm("crm.supportInbox.filters.allConnections", locale),
            icon: <MailCheck aria-hidden="true" className="size-4" />,
            tone: activeConnectionCount > 0 ? "success" : "neutral",
          },
          {
            label: tCrm("crm.supportInbox.messages.title", locale),
            value: messages.totalCount,
            description: tCrm("crm.supportInbox.messages.description", locale),
            icon: <Inbox aria-hidden="true" className="size-4" />,
          },
          {
            label: tCrm("crm.supportInbox.fields.ticket", locale),
            value: linkedMessagesOnPage,
            description: tCrm("crm.supportInbox.filters.ticketLink", locale),
            icon: <Link2 aria-hidden="true" className="size-4" />,
            tone: linkedToTicket === false ? "warning" : "neutral",
          },
        ]}
      />

      <div className="grid gap-4 xl:grid-cols-[minmax(0,0.9fr)_minmax(22rem,1.1fr)]">
        <CrmSectionCard
          title={tCrm("crm.supportInbox.connections.title", locale)}
          description={tCrm("crm.supportInbox.connections.description", locale)}
          actions={<Badge variant="secondary">{activeConnectionCount}</Badge>}
          contentClassName="p-0"
        >
          <ConnectionsTable connections={connections} locale={locale} />
        </CrmSectionCard>

        <CrmToolbarSurface
          title={tCrm("crm.supportInbox.messages.title", locale)}
          description={tCrm("crm.supportInbox.messages.description", locale)}
        >
          <SupportInboxFilterForm
            connections={connections}
            connectionId={connectionId}
            linkedToTicketValue={linkedToTicketValue}
            pageSize={pageSize}
          />
        </CrmToolbarSurface>
      </div>

      <CrmSectionCard
        title={tCrm("crm.supportInbox.messages.title", locale)}
        description={tCrm("crm.supportInbox.messages.description", locale)}
        actions={
          <Badge variant="outline">
            {messages.items.length} / {messages.totalCount}
          </Badge>
        }
        contentClassName="p-0"
      >
        <MessagesTable dateSettings={dateSettings} locale={locale} messages={messages.items} />
      </CrmSectionCard>

      <CrmPagination
        currentPage={messages.pageNumber}
        totalPages={messages.totalPages}
        basePath="/support-inbox"
        currentQuery={currentQuery}
      />
    </CrmPageShell>
  );
}

function ConnectionsTable({
  connections,
  locale,
}: Readonly<{ connections: SupportInboxConnectionDto[]; locale: string }>) {
  const rows: CrmRecordsTableRow[] = connections.map((connection) => {
    const state = connection.isActive
      ? tCrm("crm.supportInbox.state.active", locale)
      : tCrm("crm.supportInbox.state.inactive", locale);

    return {
      id: connection.id,
      cells: {
        name: connection.name,
        provider: String(connection.provider),
        host: connection.host,
        state,
      },
      descriptions: {
        name: connection.emailAddress,
        host: `${connection.port} / ${connection.useSsl ? "SSL" : "Plain"}`,
      },
      searchText: [
        connection.name,
        connection.emailAddress,
        connection.provider,
        connection.host,
        connection.port,
        state,
      ]
        .filter(Boolean)
        .join(" "),
      filterValues: {
        state,
      },
    };
  });

  return (
    <CrmRecordsTable
      caption={tCrm("crm.supportInbox.connections.caption", locale)}
      columns={[
        { key: "name", header: tCrm("crm.supportInbox.fields.name", locale) },
        { key: "provider", header: tCrm("crm.supportInbox.fields.provider", locale) },
        { key: "host", header: tCrm("crm.supportInbox.fields.host", locale) },
        { key: "state", header: tCrm("crm.supportInbox.fields.state", locale), badge: true },
      ]}
      rows={rows}
      filters={[
        {
          key: "state",
          label: tCrm("crm.supportInbox.fields.state", locale),
          allLabel: `All ${tCrm("crm.supportInbox.fields.state", locale).toLocaleLowerCase()}`,
          options: [...new Set(rows.map((row) => row.cells.state))]
            .filter((value): value is string => Boolean(value))
            .map((value) => ({
              value,
              label: value,
            })),
        },
      ]}
      labels={{
        searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
        emptyTitle: tCrm("crm.supportInbox.connections.emptyTitle", locale),
        emptyDescription: tCrm("crm.supportInbox.connections.emptyDescription", locale),
      }}
      minWidthClassName="min-w-[760px]"
    />
  );
}

function MessagesTable({
  dateSettings,
  locale,
  messages,
}: Readonly<{
  dateSettings: Awaited<ReturnType<typeof getRequestDateSettings>>;
  locale: string;
  messages: SupportInboxMessageDto[];
}>) {
  const rows: CrmRecordsTableRow[] = messages.map((message) => {
    const status = tCrmWithFallback(
      `crm.supportInbox.status.${message.status}`,
      message.status,
      locale,
    );
    const ticket = message.ticketId ?? "-";

    return {
      id: message.id,
      cells: {
        received: formatCrmDateTime(message.receivedAtUtc, dateSettings),
        from: message.fromAddress,
        subject: message.subject,
        status,
        ticket,
      },
      descriptions: {
        subject: message.externalMessageId,
      },
      searchText: [
        formatCrmDateTime(message.receivedAtUtc, dateSettings),
        message.fromAddress,
        message.subject,
        message.externalMessageId,
        status,
        ticket,
      ]
        .filter(Boolean)
        .join(" "),
      filterValues: {
        status,
        ticket: message.ticketId ? "Linked" : "Unlinked",
      },
    };
  });

  return (
    <CrmRecordsTable
      caption={tCrm("crm.supportInbox.messages.caption", locale)}
      columns={[
        { key: "received", header: tCrm("crm.supportInbox.fields.received", locale) },
        { key: "from", header: tCrm("crm.supportInbox.fields.from", locale) },
        { key: "subject", header: tCrm("crm.supportInbox.fields.subject", locale) },
        { key: "status", header: tCrm("crm.supportInbox.fields.status", locale), badge: true },
        { key: "ticket", header: tCrm("crm.supportInbox.fields.ticket", locale), badge: true },
      ]}
      rows={rows}
      filters={[
        {
          key: "status",
          label: tCrm("crm.supportInbox.fields.status", locale),
          allLabel: `All ${tCrm("crm.supportInbox.fields.status", locale).toLocaleLowerCase()}`,
          options: [...new Set(rows.map((row) => row.cells.status))]
            .filter((value): value is string => Boolean(value))
            .map((value) => ({
              value,
              label: value,
            })),
        },
      ]}
      labels={{
        searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
        emptyTitle: tCrm("crm.supportInbox.messages.emptyTitle", locale),
        emptyDescription: tCrm("crm.supportInbox.messages.emptyDescription", locale),
      }}
      minWidthClassName="min-w-[880px]"
    />
  );
}
