import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { LeadBulkActionsPanel } from "@/features/leads/components/lead-bulk-actions-panel";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function LeadBulkOperationsPage() {
  const session = await requireCrmSession("/leads/bulk-operations");
  const locale = await getRequestLocale();

  return (
    <CrmPageShell
      title={tCrm("crm.leads.bulkOperations.title", locale)}
      description={tCrm("crm.leads.bulkOperations.description", locale)}
      locale={locale}
      actions={
        <Button asChild variant="outline">
          <Link href="/leads">
            <ArrowLeft aria-hidden="true" />
            {tCrm("crm.common.backToList", locale)}
          </Link>
        </Button>
      }
    >
      <LeadBulkActionsPanel
        canAssign={crmCapabilityAllows(session.capabilities, "leads.edit")}
        canDelete={crmCapabilityAllows(session.capabilities, "leads.delete")}
        locale={locale}
      />
    </CrmPageShell>
  );
}
