import { notFound } from "next/navigation";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getLeadDetailData } from "@/features/leads/data/leads-data";
import { LeadForm } from "@/features/leads/forms/lead-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditLeadPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  await requireCrmSession(`/leads/${resolved.id}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let lead;

  try {
    lead = await getLeadDetailData(resolved.id, `/leads/${resolved.id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/leads/${resolved.id}/edit`);
  }

  return (
    <CrmEntityFormShell routePath="/leads/[id]/edit" locale={locale}>
      <LeadForm
        mode="edit"
        leadId={resolved.id}
        initialValues={{
          fullName: lead.fullName,
          companyName: lead.companyName ?? "",
          email: lead.email ?? "",
          phone: lead.phone ?? "",
          jobTitle: lead.jobTitle ?? "",
          description: lead.description ?? "",
          estimatedBudget: lead.estimatedBudget?.toString() ?? "",
          nextContactDate: lead.nextContactDate?.slice(0, 10) ?? "",
          source: Number(lead.source),
          status: Number(lead.status),
          priority: Number(lead.priority),
          companyId: lead.companyId ?? "",
          ownerUserId: lead.ownerUserId ?? "",
          notes: lead.notes ?? "",
          rowVersion: lead.rowVersion,
        }}
        customerOptions={references.customers}
        companyOptions={references.companies}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
