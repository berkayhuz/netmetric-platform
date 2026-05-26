import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import {
  createSupportInboxConnectionAction,
  createSupportInboxRuleAction,
  triggerSupportInboxSyncAction,
  updateSupportInboxConnectionAction,
  updateSupportInboxRuleAction,
} from "@/features/support-inbox/actions/support-inbox-mutation-actions";
import { SupportInboxMutationPanels } from "@/features/support-inbox/components/support-inbox-mutation-panels";
import { getSupportInboxData } from "@/features/support-inbox/data/support-inbox-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function SupportInboxOperationsPage() {
  await requireCrmSession("/support-inbox/operations");
  const locale = await getRequestLocale();
  const { connections } = await getSupportInboxData(
    { page: 1, pageSize: 20 },
    "/support-inbox/operations",
  );

  return (
    <CrmPageShell
      title={tCrm("crm.supportInbox.operations.title", locale)}
      description={tCrm("crm.supportInbox.operations.description", locale)}
      locale={locale}
      actions={
        <Button asChild variant="outline">
          <Link href="/support-inbox">
            <ArrowLeft aria-hidden="true" />
            {tCrm("crm.common.backToList", locale)}
          </Link>
        </Button>
      }
    >
      <SupportInboxMutationPanels
        connections={connections}
        createConnectionAction={createSupportInboxConnectionAction}
        updateConnectionAction={updateSupportInboxConnectionAction}
        syncConnectionAction={triggerSupportInboxSyncAction}
        createRuleAction={createSupportInboxRuleAction}
        updateRuleAction={updateSupportInboxRuleAction}
      />
    </CrmPageShell>
  );
}
