import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { CustomerForm } from "@/features/customers/forms/customer-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewCustomerPage() {
  await requireCrmSession("/customers/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/customers/new" locale={locale}>
      <CustomerForm
        mode="create"
        companyOptions={references.companies}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
