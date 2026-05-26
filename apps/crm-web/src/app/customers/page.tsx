import { CrmEntityListPage } from "@/components/shell/crm-entity-list-page";
import { CrmPageHeaderActionLink } from "@/components/shell/crm-page-header-actions";
import type { CrmEntityTableColumn } from "@/components/shell/crm-table.types";
import { getCustomersData } from "@/features/customers/data/customers-data";
import { deleteCustomersBulkFromListAction } from "@/features/customers/actions/customer-mutation-actions";
import { toListQuery } from "@/features/shared/data/query";
import type { CrmListQuery, CustomerListItemDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function CustomersPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/customers");
  const locale = await getRequestLocale();
  const params = await searchParams;
  const baseQuery = toListQuery(params);
  const allowedCustomerSortFields = new Set(["name", "email", "createdAt", "isVip", "company"]);
  const query: CrmListQuery =
    baseQuery.sortBy && allowedCustomerSortFields.has(baseQuery.sortBy)
      ? baseQuery
      : (({ sortBy: _sortBy, sortDirection: _sortDirection, ...rest }) => rest)(baseQuery);
  const paged = await getCustomersData(query, "/customers");
  const canCreate = crmCapabilityAllows(session.capabilities, "customers.create");
  const canDelete = crmCapabilityAllows(session.capabilities, "customers.delete");
  const canImport = crmCapabilityAllows(session.capabilities, "canImportCustomer");
  const columns: CrmEntityTableColumn<CustomerListItemDto>[] = [
    {
      key: "fullName",
      header: tCrm("crm.customers.fields.name", locale),
      render: (item) => item.fullName,
    },
    {
      key: "email",
      header: tCrm("crm.customers.fields.email", locale),
      render: (item) => item.email || "-",
    },
    {
      key: "mobilePhone",
      header: tCrm("crm.customers.fields.mobilePhoneShort", locale),
      render: (item) => item.mobilePhone || "-",
    },
    {
      key: "companyName",
      header: tCrm("crm.customers.fields.company", locale),
      render: (item) => item.companyName || "-",
    },
    {
      key: "customerType",
      header: tCrm("crm.customers.fields.customerType", locale),
      render: (item) => customerTypeLabel(item.customerType, locale),
    },
    {
      key: "isActive",
      header: tCrm("crm.customers.fields.status", locale),
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
      routePath="/customers"
      actionPath="/customers"
      locale={locale}
      createPath="/customers/new"
      createLabel={tCrm("crm.customers.actions.new", locale)}
      canCreate={canCreate}
      canDelete={canDelete}
      bulkDeleteAction={deleteCustomersBulkFromListAction}
      createDisabledMessage={tCrm("crm.states.readOnly", locale)}
      secondaryActions={
        canImport ? (
          <CrmPageHeaderActionLink
            href="/customers/imports"
            label={tCrm("crm.customers.pages.list.importTitle", locale)}
            variant="outline"
          />
        ) : null
      }
      {...(query.search ? { search: query.search } : {})}
      caption={tCrm("crm.customers.pages.list.caption", locale)}
      columns={columns}
      paged={paged}
      detailBasePath="/customers"
      currentQuery={currentQuery}
      emptyTitle={tCrm("crm.customers.states.emptyTitle", locale)}
      emptyDescription={tCrm("crm.customers.states.emptyDescription", locale)}
    />
  );
}

function customerTypeLabel(customerType: string | number, locale: string): string {
  const normalized = String(customerType).toLowerCase();
  return normalized === "1" || normalized === "corporate"
    ? tCrm("crm.customers.options.customerType.corporate", locale)
    : tCrm("crm.customers.options.customerType.individual", locale);
}
