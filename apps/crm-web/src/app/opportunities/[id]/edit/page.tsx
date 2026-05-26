import { notFound } from "next/navigation";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getOpportunityDetailData } from "@/features/opportunities/data/opportunities-data";
import { OpportunityForm } from "@/features/opportunities/forms/opportunity-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditOpportunityPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  await requireCrmSession(`/opportunities/${resolved.id}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let opportunity;

  try {
    opportunity = await getOpportunityDetailData(resolved.id, `/opportunities/${resolved.id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/opportunities/${resolved.id}/edit`);
  }

  return (
    <CrmEntityFormShell routePath="/opportunities/[id]/edit" locale={locale}>
      <OpportunityForm
        mode="edit"
        opportunityId={resolved.id}
        initialValues={{
          opportunityCode: opportunity.opportunityCode,
          name: opportunity.name,
          description: opportunity.description ?? "",
          estimatedAmount: (opportunity.estimatedAmount ?? 0).toString(),
          expectedRevenue: opportunity.expectedRevenue?.toString() ?? "",
          probability: Number(opportunity.probability),
          estimatedCloseDate: opportunity.estimatedCloseDate?.slice(0, 10) ?? "",
          stage: Number(opportunity.stage),
          status: Number(opportunity.status),
          priority: Number(opportunity.priority),
          leadId: opportunity.leadId ?? "",
          customerId: opportunity.customerId ?? "",
          ownerUserId: opportunity.ownerUserId ?? "",
          notes: opportunity.notes ?? "",
          rowVersion: opportunity.rowVersion,
        }}
        leadOptions={references.leads}
        customerOptions={references.customers}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
