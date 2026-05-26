import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { DealBulkActionsPanel } from "@/features/deals/components/deal-bulk-actions-panel";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function DealBulkOperationsPage() {
  const session = await requireCrmSession("/deals/bulk-operations");
  const locale = await getRequestLocale();

  return (
    <CrmPageShell
      title={tCrm("crm.deals.bulkOperations.title", locale)}
      description={tCrm("crm.deals.bulkOperations.description", locale)}
      locale={locale}
      actions={
        <Button asChild variant="outline">
          <Link href="/deals">
            <ArrowLeft aria-hidden="true" />
            {tCrm("crm.common.backToList", locale)}
          </Link>
        </Button>
      }
    >
      <DealBulkActionsPanel
        canAssign={crmCapabilityAllows(session.capabilities, "deals.edit")}
        locale={locale}
      />
    </CrmPageShell>
  );
}
