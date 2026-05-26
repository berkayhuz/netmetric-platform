import { CrmEntityListPage } from "@/components/shell/crm-entity-list-page";
import { CrmBulkOperationsHeaderAction } from "@/components/shell/crm-bulk-operations-header-action";
import type { CrmEntityTableColumn } from "@/components/shell/crm-table.types";
import { getDealsData } from "@/features/deals/data/deals-data";
import { deleteDealsBulkFromListAction } from "@/features/deals/actions/deal-mutation-actions";
import { toListQuery } from "@/features/shared/data/query";
import type { DealListItemDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDate } from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function DealsPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/deals");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const capabilities = session.capabilities;

  const params = await searchParams;
  const query = toListQuery(params);
  const data = await getDealsData(query, "/deals");

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }

  const columns: CrmEntityTableColumn<DealListItemDto>[] = [
    {
      key: "dealCode",
      header: tCrm("crm.deals.fields.dealCode", locale),
      render: (item) => item.dealCode,
    },
    { key: "name", header: tCrm("crm.deals.fields.name", locale), render: (item) => item.name },
    {
      key: "totalAmount",
      header: tCrm("crm.deals.fields.totalAmount", locale),
      render: (item) => (item.totalAmount ?? "-") as string | number,
    },
    {
      key: "closedDate",
      header: tCrm("crm.deals.fields.closedDate", locale),
      render: (item) => formatCrmDate(item.closedDate, dateSettings),
    },
    {
      key: "stage",
      header: tCrm("crm.deals.fields.stage", locale),
      render: (item) => tCrm(`crm.deals.stage.${item.stage}`, locale),
    },
    {
      key: "outcome",
      header: tCrm("crm.deals.fields.outcome", locale),
      render: (item) => String(item.outcome),
    },
    {
      key: "isActive",
      header: tCrm("crm.deals.fields.state", locale),
      render: (item) =>
        item.isActive ? tCrm("crm.common.active", locale) : tCrm("crm.common.inactive", locale),
    },
  ];

  return (
    <CrmEntityListPage
      routePath="/deals"
      actionPath="/deals"
      createPath="/deals/new"
      createLabel={tCrm("crm.deals.actions.create", locale)}
      canCreate={crmCapabilityAllows(capabilities, "deals.create")}
      canDelete={crmCapabilityAllows(capabilities, "deals.delete")}
      bulkDeleteAction={deleteDealsBulkFromListAction}
      createDisabledMessage={tCrm("crm.states.readOnly", locale)}
      secondaryActions={<CrmBulkOperationsHeaderAction basePath="/deals" locale={locale} />}
      {...(query.search ? { search: query.search } : {})}
      caption={tCrm("crm.deals.pages.list.caption", locale)}
      columns={columns}
      paged={data}
      detailBasePath="/deals"
      currentQuery={currentQuery}
      locale={locale}
    />
  );
}
