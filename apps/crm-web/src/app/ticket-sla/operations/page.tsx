import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import {
  createTicketSlaEscalationRuleAction,
  createTicketSlaPolicyAction,
  deleteTicketSlaPolicyAction,
  updateTicketSlaEscalationRuleAction,
  updateTicketSlaPolicyAction,
} from "@/features/ticket-sla/actions/ticket-sla-mutation-actions";
import { TicketSlaMutationPanels } from "@/features/ticket-sla/forms/ticket-sla-mutation-panels";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function TicketSlaOperationsPage() {
  await requireCrmSession("/ticket-sla/operations");
  const locale = await getRequestLocale();

  return (
    <CrmPageShell
      title={tCrm("crm.ticketSla.mutations.title", locale)}
      description={tCrm("crm.ticketSla.mutations.description", locale)}
      locale={locale}
      actions={
        <Button asChild variant="outline">
          <Link href="/ticket-sla">
            <ArrowLeft aria-hidden="true" />
            {tCrm("crm.common.backToList", locale)}
          </Link>
        </Button>
      }
    >
      <TicketSlaMutationPanels
        createPolicyAction={createTicketSlaPolicyAction}
        updatePolicyAction={updateTicketSlaPolicyAction}
        deletePolicyAction={deleteTicketSlaPolicyAction}
        createRuleAction={createTicketSlaEscalationRuleAction}
        updateRuleAction={updateTicketSlaEscalationRuleAction}
      />
    </CrmPageShell>
  );
}
