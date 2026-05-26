import { notFound } from "next/navigation";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getDealDetailData } from "@/features/deals/data/deals-data";
import { DealForm } from "@/features/deals/forms/deal-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditDealPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  await requireCrmSession(`/deals/${resolved.id}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let deal;

  try {
    deal = await getDealDetailData(resolved.id, `/deals/${resolved.id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/deals/${resolved.id}/edit`);
  }

  return (
    <CrmEntityFormShell routePath="/deals/[id]/edit" locale={locale}>
      <DealForm
        mode="edit"
        dealId={resolved.id}
        initialValues={{
          dealCode: deal.dealCode,
          name: deal.name,
          totalAmount: deal.totalAmount?.toString() ?? "",
          closedDate: deal.closedDate.slice(0, 10),
          opportunityId: deal.opportunityId ?? "",
          companyId: deal.companyId ?? "",
          ownerUserId: deal.ownerUserId ?? "",
          notes: "",
          rowVersion: deal.rowVersion,
        }}
        companyOptions={references.companies}
        opportunityOptions={references.opportunities}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
