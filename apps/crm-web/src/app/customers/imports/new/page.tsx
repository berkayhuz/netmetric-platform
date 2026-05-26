import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CustomerImportPanel } from "@/features/customers/components/customer-import-panel";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewCustomerImportBatchPage() {
  const session = await requireCrmSession("/customers/imports");
  const locale = await getRequestLocale();
  const canImport = crmCapabilityAllows(session.capabilities, "canImportCustomer");

  return (
    <CrmPageShell
      title={tCrm("crm.customers.actions.createImportBatch", locale)}
      description={tCrm("crm.customers.pages.list.importDescription", locale)}
      locale={locale}
      actions={
        <Button asChild variant="outline">
          <Link href="/customers/imports">
            <ArrowLeft aria-hidden="true" />
            {tCrm("crm.common.backToList", locale)}
          </Link>
        </Button>
      }
    >
      <CustomerImportPanel batches={[]} canImport={canImport} locale={locale} />
    </CrmPageShell>
  );
}
