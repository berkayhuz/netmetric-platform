import { notFound } from "next/navigation";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { isGuid } from "@/features/shared/data/guid";
import { getTicketDetailData } from "@/features/tickets/data/tickets-data";
import { TicketForm } from "@/features/tickets/forms/ticket-form";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

function toDateTimeLocal(value?: string | null): string {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.valueOf())) {
    return "";
  }

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hour = String(date.getHours()).padStart(2, "0");
  const minute = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hour}:${minute}`;
}

export default async function EditTicketPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  await requireCrmSession(`/tickets/${resolved.id}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let ticket;

  try {
    ticket = await getTicketDetailData(resolved.id, `/tickets/${resolved.id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/tickets/${resolved.id}/edit`);
  }

  return (
    <CrmEntityFormShell routePath="/tickets/[id]/edit" locale={locale}>
      <TicketForm
        mode="edit"
        ticketId={resolved.id}
        initialValues={{
          subject: ticket.subject,
          description: ticket.description ?? "",
          ticketType: Number(ticket.ticketType),
          channel: Number(ticket.channel),
          priority: Number(ticket.priority),
          assignedUserId: ticket.assignedUserId ?? "",
          customerId: ticket.customerId ?? "",
          contactId: ticket.contactId ?? "",
          ticketCategoryId: ticket.ticketCategoryId ?? "",
          slaPolicyId: ticket.slaPolicyId ?? "",
          firstResponseDueAt: toDateTimeLocal(ticket.firstResponseDueAt),
          resolveDueAt: toDateTimeLocal(ticket.resolveDueAt),
          notes: ticket.notes ?? "",
          rowVersion: ticket.rowVersion,
        }}
        customerOptions={references.customers}
        contactOptions={references.contacts}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
