import { CrmEntityListPage } from "@/components/shell/crm-entity-list-page";
import type { CrmEntityTableColumn } from "@/components/shell/crm-table.types";
import { getTicketsData } from "@/features/tickets/data/tickets-data";
import { deleteTicketsBulkFromListAction } from "@/features/tickets/actions/ticket-mutation-actions";
import { toListQuery } from "@/features/shared/data/query";
import type { TicketListItemDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDate, type CrmDateSettings } from "@/lib/date-time/crm-date-time";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";

function getColumns(dateSettings: CrmDateSettings): CrmEntityTableColumn<TicketListItemDto>[] {
  const locale = dateSettings.locale;
  return [
    {
      key: "ticketNumber",
      header: tCrm("crm.tickets.fields.ticketNumber", locale),
      render: (item) => item.ticketNumber,
    },
    {
      key: "subject",
      header: tCrm("crm.tickets.fields.subject", locale),
      render: (item) => item.subject,
    },
    {
      key: "priority",
      header: tCrm("crm.tickets.fields.priority", locale),
      render: (item) =>
        tCrmWithFallback(`crm.common.priority.${item.priority}`, String(item.priority), locale),
    },
    {
      key: "status",
      header: tCrm("crm.tickets.fields.status", locale),
      render: (item) =>
        tCrmWithFallback(`crm.tickets.status.${item.status}`, String(item.status), locale),
    },
    {
      key: "openedAt",
      header: tCrm("crm.tickets.fields.opened", locale),
      render: (item) => formatCrmDate(item.openedAt, dateSettings),
    },
    {
      key: "assignedUserId",
      header: tCrm("crm.tickets.fields.assignedUser", locale),
      render: (item) => item.assignedUserId || "-",
    },
    {
      key: "isActive",
      header: tCrm("crm.tickets.fields.state", locale),
      render: (item) =>
        item.isActive ? tCrm("crm.states.active", locale) : tCrm("crm.states.inactive", locale),
    },
  ];
}

export default async function TicketsPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/tickets");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const capabilities = session.capabilities;

  const params = await searchParams;
  const query = toListQuery(params);
  const data = await getTicketsData(query, "/tickets");

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }

  return (
    <CrmEntityListPage
      routePath="/tickets"
      actionPath="/tickets"
      createPath="/tickets/new"
      createLabel={tCrm("crm.tickets.actions.create", locale)}
      canCreate={crmCapabilityAllows(capabilities, "tickets.create")}
      canDelete={crmCapabilityAllows(capabilities, "tickets.delete")}
      bulkDeleteAction={deleteTicketsBulkFromListAction}
      createDisabledMessage={tCrm("crm.states.readOnly", locale)}
      {...(query.search ? { search: query.search } : {})}
      caption={tCrm("crm.tickets.caption", locale)}
      columns={getColumns(dateSettings)}
      paged={data}
      detailBasePath="/tickets"
      currentQuery={currentQuery}
      locale={locale}
    />
  );
}
