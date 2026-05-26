import { CrmEntityListPage } from "@/components/shell/crm-entity-list-page";
import type { CrmEntityTableColumn } from "@/components/shell/crm-table.types";
import { getCompaniesData } from "@/features/companies/data/companies-data";
import { deleteCompaniesBulkFromListAction } from "@/features/companies/actions/company-mutation-actions";
import { toListQuery } from "@/features/shared/data/query";
import type { CompanyListItemDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function CompaniesPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/companies");
  const locale = await getRequestLocale();
  const capabilities = session.capabilities;

  const params = await searchParams;
  const query = toListQuery(params);
  const data = await getCompaniesData(query, "/companies");
  const columns: CrmEntityTableColumn<CompanyListItemDto>[] = [
    { key: "name", header: tCrm("crm.companies.fields.name", locale), render: (item) => item.name },
    {
      key: "email",
      header: tCrm("crm.companies.fields.email", locale),
      render: (item) => item.email || "-",
    },
    {
      key: "phone",
      header: tCrm("crm.companies.fields.phone", locale),
      render: (item) => item.phone || "-",
    },
    {
      key: "sector",
      header: tCrm("crm.companies.fields.sector", locale),
      render: (item) => item.sector || "-",
    },
    {
      key: "contactCount",
      header: tCrm("crm.companies.fields.contacts", locale),
      render: (item) => item.contactCount,
    },
    {
      key: "isActive",
      header: tCrm("crm.companies.fields.status", locale),
      render: (item) =>
        item.isActive ? tCrm("crm.common.active", locale) : tCrm("crm.common.inactive", locale),
    },
  ];

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }

  return (
    <CrmEntityListPage
      routePath="/companies"
      actionPath="/companies"
      createPath="/companies/new"
      createLabel={tCrm("crm.companies.actions.new", locale)}
      canCreate={crmCapabilityAllows(capabilities, "companies.create")}
      canDelete={crmCapabilityAllows(capabilities, "companies.delete")}
      bulkDeleteAction={deleteCompaniesBulkFromListAction}
      createDisabledMessage={tCrm("crm.states.readOnly", locale)}
      {...(query.search ? { search: query.search } : {})}
      caption={tCrm("crm.companies.pages.list.caption", locale)}
      columns={columns}
      paged={data}
      detailBasePath="/companies"
      currentQuery={currentQuery}
      locale={locale}
      emptyTitle={tCrm("crm.companies.states.emptyTitle", locale)}
      emptyDescription={tCrm("crm.companies.states.emptyDescription", locale)}
    />
  );
}
