import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { TicketForm } from "@/features/tickets/forms/ticket-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewTicketPage() {
  await requireCrmSession("/tickets/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/tickets/new" locale={locale}>
      <TicketForm
        mode="create"
        customerOptions={references.customers}
        contactOptions={references.contacts}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
