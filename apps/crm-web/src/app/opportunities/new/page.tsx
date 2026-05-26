import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { OpportunityForm } from "@/features/opportunities/forms/opportunity-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewOpportunityPage() {
  await requireCrmSession("/opportunities/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/opportunities/new" locale={locale}>
      <OpportunityForm
        mode="create"
        leadOptions={references.leads}
        customerOptions={references.customers}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
