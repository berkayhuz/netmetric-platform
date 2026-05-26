import { CrmEntityListPage } from "@/components/shell/crm-entity-list-page";
import type { CrmEntityTableColumn } from "@/components/shell/crm-table.types";
import { getQuotesData } from "@/features/quotes/data/quotes-data";
import { deleteQuotesBulkFromListAction } from "@/features/quotes/actions/quote-mutation-actions";
import { toListQuery } from "@/features/shared/data/query";
import type { QuoteListItemDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDate, type CrmDateSettings } from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

function getColumns(dateSettings: CrmDateSettings): CrmEntityTableColumn<QuoteListItemDto>[] {
  const locale = dateSettings.locale;
  return [
    {
      key: "quoteNumber",
      header: tCrm("crm.quotes.columns.quoteNumber", locale),
      render: (item) => item.quoteNumber,
    },
    {
      key: "proposalTitle",
      header: tCrm("crm.quotes.columns.proposalTitle", locale),
      render: (item) => item.proposalTitle || "-",
    },
    {
      key: "status",
      header: tCrm("crm.quotes.columns.status", locale),
      render: (item) =>
        tCrmWithFallback(`crm.quotes.status.${item.status}`, String(item.status), locale),
    },
    {
      key: "quoteDate",
      header: tCrm("crm.quotes.columns.quoteDate", locale),
      render: (item) => formatCrmDate(item.quoteDate, dateSettings),
    },
    {
      key: "validUntil",
      header: tCrm("crm.quotes.columns.validUntil", locale),
      render: (item) => formatCrmDate(item.validUntil, dateSettings),
    },
    {
      key: "currencyCode",
      header: tCrm("crm.quotes.columns.currency", locale),
      render: (item) => item.currencyCode,
    },
    {
      key: "grandTotal",
      header: tCrm("crm.quotes.columns.grandTotal", locale),
      render: (item) => item.grandTotal ?? "-",
    },
    {
      key: "isActive",
      header: tCrm("crm.common.columns.state", locale),
      render: (item) =>
        item.isActive ? tCrm("crm.states.active", locale) : tCrm("crm.states.inactive", locale),
    },
  ];
}

export default async function QuotesPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/quotes");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const capabilities = session.capabilities;

  const params = await searchParams;
  const query = toListQuery(params);
  const data = await getQuotesData(query, "/quotes");

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }

  return (
    <CrmEntityListPage
      routePath="/quotes"
      actionPath="/quotes"
      createPath="/quotes/new"
      createLabel={tCrm("crm.quotes.actions.create", locale)}
      canCreate={crmCapabilityAllows(capabilities, "quotes.create")}
      canDelete={crmCapabilityAllows(capabilities, "quotes.delete")}
      bulkDeleteAction={deleteQuotesBulkFromListAction}
      createDisabledMessage={tCrm("crm.states.readOnly", locale)}
      {...(query.search ? { search: query.search } : {})}
      caption={tCrm("crm.quotes.caption", locale)}
      columns={getColumns(dateSettings)}
      paged={data}
      detailBasePath="/quotes"
      currentQuery={currentQuery}
      locale={locale}
    />
  );
}
