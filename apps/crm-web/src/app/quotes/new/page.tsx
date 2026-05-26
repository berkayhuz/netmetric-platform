import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getProposalTemplatesData } from "@/features/quotes/data/quotes-data";
import { QuoteForm } from "@/features/quotes/forms/quote-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewQuotePage() {
  await requireCrmSession("/quotes/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();
  const proposalTemplates = await getProposalTemplatesData("/quotes/new", true);
  const proposalTemplateOptions = proposalTemplates.map((template) => ({
    value: template.id,
    label: template.name,
  }));

  return (
    <CrmEntityFormShell routePath="/quotes/new" locale={locale}>
      <QuoteForm
        mode="create"
        opportunityOptions={references.opportunities}
        customerOptions={references.customers}
        ownerUserOptions={references.ownerUsers}
        proposalTemplateOptions={proposalTemplateOptions}
        productOptions={references.products}
      />
    </CrmEntityFormShell>
  );
}
