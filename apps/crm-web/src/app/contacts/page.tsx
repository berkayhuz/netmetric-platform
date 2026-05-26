import { CrmEntityListPage } from "@/components/shell/crm-entity-list-page";
import type { CrmEntityTableColumn } from "@/components/shell/crm-table.types";
import { getContactsData } from "@/features/contacts/data/contacts-data";
import { deleteContactsBulkFromListAction } from "@/features/contacts/actions/contact-mutation-actions";
import { toListQuery } from "@/features/shared/data/query";
import type { ContactListItemDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function ContactsPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/contacts");
  const locale = await getRequestLocale();
  const capabilities = session.capabilities;

  const params = await searchParams;
  const query = toListQuery(params);
  const data = await getContactsData(query, "/contacts");
  const columns: CrmEntityTableColumn<ContactListItemDto>[] = [
    {
      key: "fullName",
      header: tCrm("crm.contacts.fields.name", locale),
      render: (item) => item.fullName,
    },
    {
      key: "email",
      header: tCrm("crm.contacts.fields.email", locale),
      render: (item) => item.email || "-",
    },
    {
      key: "mobilePhone",
      header: tCrm("crm.contacts.fields.mobilePhoneShort", locale),
      render: (item) => item.mobilePhone || "-",
    },
    {
      key: "companyName",
      header: tCrm("crm.contacts.fields.company", locale),
      render: (item) => item.companyName || "-",
    },
    {
      key: "customerName",
      header: tCrm("crm.contacts.fields.customer", locale),
      render: (item) => item.customerName || "-",
    },
    {
      key: "isPrimaryContact",
      header: tCrm("crm.contacts.fields.primaryContact", locale),
      render: (item) =>
        item.isPrimaryContact ? tCrm("crm.common.yes", locale) : tCrm("crm.common.no", locale),
    },
  ];

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }

  return (
    <CrmEntityListPage
      routePath="/contacts"
      actionPath="/contacts"
      createPath="/contacts/new"
      createLabel={tCrm("crm.contacts.actions.new", locale)}
      canCreate={crmCapabilityAllows(capabilities, "contacts.create")}
      canDelete={crmCapabilityAllows(capabilities, "contacts.delete")}
      bulkDeleteAction={deleteContactsBulkFromListAction}
      createDisabledMessage={tCrm("crm.states.readOnly", locale)}
      {...(query.search ? { search: query.search } : {})}
      caption={tCrm("crm.contacts.pages.list.caption", locale)}
      columns={columns}
      paged={data}
      detailBasePath="/contacts"
      currentQuery={currentQuery}
      locale={locale}
      emptyTitle={tCrm("crm.contacts.states.emptyTitle", locale)}
      emptyDescription={tCrm("crm.contacts.states.emptyDescription", locale)}
    />
  );
}
