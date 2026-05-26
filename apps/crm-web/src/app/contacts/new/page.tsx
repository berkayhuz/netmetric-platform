import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { ContactForm } from "@/features/contacts/forms/contact-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewContactPage() {
  await requireCrmSession("/contacts/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/contacts/new" locale={locale}>
      <ContactForm
        mode="create"
        companyOptions={references.companies}
        customerOptions={references.customers}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
