import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { DealForm } from "@/features/deals/forms/deal-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewDealPage() {
  await requireCrmSession("/deals/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/deals/new" locale={locale}>
      <DealForm
        mode="create"
        companyOptions={references.companies}
        opportunityOptions={references.opportunities}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
