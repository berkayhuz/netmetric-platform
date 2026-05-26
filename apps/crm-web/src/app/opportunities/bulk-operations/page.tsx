import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { OpportunityBulkActionsPanel } from "@/features/opportunities/components/opportunity-bulk-actions-panel";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function OpportunityBulkOperationsPage() {
  const session = await requireCrmSession("/opportunities/bulk-operations");
  const locale = await getRequestLocale();

  return (
    <CrmPageShell
      title={tCrm("crm.opportunities.bulkOperations.title", locale)}
      description={tCrm("crm.opportunities.bulkOperations.description", locale)}
      locale={locale}
      actions={
        <Button asChild variant="outline">
          <Link href="/opportunities">
            <ArrowLeft aria-hidden="true" />
            {tCrm("crm.common.backToList", locale)}
          </Link>
        </Button>
      }
    >
      <OpportunityBulkActionsPanel
        canManage={crmCapabilityAllows(session.capabilities, "opportunities.edit")}
        locale={locale}
      />
    </CrmPageShell>
  );
}
