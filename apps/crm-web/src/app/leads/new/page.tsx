import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { LeadForm } from "@/features/leads/forms/lead-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewLeadPage() {
  await requireCrmSession("/leads/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/leads/new" locale={locale}>
      <LeadForm
        mode="create"
        customerOptions={references.customers}
        companyOptions={references.companies}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
