import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { CompanyForm } from "@/features/companies/forms/company-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewCompanyPage() {
  await requireCrmSession("/companies/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/companies/new" locale={locale}>
      <CompanyForm
        mode="create"
        companyOptions={references.companies}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
