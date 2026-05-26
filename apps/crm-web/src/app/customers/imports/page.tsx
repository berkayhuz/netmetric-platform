import { Plus } from "lucide-react";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CrmPageHeaderActionLink } from "@/components/shell/crm-page-header-actions";
import { CustomerImportsListTable } from "@/features/customers/components/customer-imports-list-table";
import { getCustomerImportBatchesData } from "@/features/customers/data/customers-data";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function CustomerImportsPage() {
  const session = await requireCrmSession("/customers/imports");
  const locale = await getRequestLocale();
  const canImport = crmCapabilityAllows(session.capabilities, "canImportCustomer");
  const importBatches = canImport ? await getCustomerImportBatchesData("/customers/imports") : [];

  return (
    <CrmPageShell
      title={tCrm("crm.customers.pages.list.importTitle", locale)}
      description={tCrm("crm.customers.pages.list.importDescription", locale)}
      locale={locale}
      actions={
        canImport ? (
          <CrmPageHeaderActionLink
            href="/customers/imports/new"
            icon={<Plus aria-hidden="true" />}
            label={tCrm("crm.customers.actions.createImportBatch", locale)}
          />
        ) : null
      }
    >
      <section className="flex h-full min-h-0 flex-col overflow-hidden">
        <CustomerImportsListTable batches={importBatches ?? []} locale={locale} />
      </section>
    </CrmPageShell>
  );
}
