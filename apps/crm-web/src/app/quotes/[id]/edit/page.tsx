import { notFound } from "next/navigation";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getProposalTemplatesData, getQuoteDetailData } from "@/features/quotes/data/quotes-data";
import { QuoteForm } from "@/features/quotes/forms/quote-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditQuotePage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  await requireCrmSession(`/quotes/${resolved.id}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();
  const proposalTemplates = await getProposalTemplatesData(`/quotes/${resolved.id}/edit`, true);
  const proposalTemplateOptions = proposalTemplates.map((template) => ({
    value: template.id,
    label: template.name,
  }));

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let quote;

  try {
    quote = await getQuoteDetailData(resolved.id, `/quotes/${resolved.id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/quotes/${resolved.id}/edit`);
  }

  return (
    <CrmEntityFormShell routePath="/quotes/[id]/edit" locale={locale}>
      <QuoteForm
        mode="edit"
        quoteId={resolved.id}
        initialValues={{
          quoteNumber: quote.quoteNumber,
          proposalTitle: quote.proposalTitle ?? "",
          proposalSummary: quote.proposalSummary ?? "",
          proposalBody: quote.proposalBody ?? "",
          quoteDate: quote.quoteDate.slice(0, 10),
          validUntil: quote.validUntil?.slice(0, 10) ?? "",
          opportunityId: quote.opportunityId ?? "",
          customerId: quote.customerId ?? "",
          ownerUserId: quote.ownerUserId ?? "",
          currencyCode: quote.currencyCode,
          exchangeRate: quote.exchangeRate?.toString() ?? "1",
          termsAndConditions: quote.termsAndConditions ?? "",
          proposalTemplateId: quote.proposalTemplateId ?? "",
          items: quote.items.map((item) => ({
            productId: item.productId,
            description: item.description ?? "",
            quantity: item.quantity,
            unitPrice: item.unitPrice.toString(),
            discountRate: item.discountRate,
            taxRate: item.taxRate,
          })),
          rowVersion: quote.rowVersion,
        }}
        opportunityOptions={references.opportunities}
        customerOptions={references.customers}
        ownerUserOptions={references.ownerUsers}
        proposalTemplateOptions={proposalTemplateOptions}
        productOptions={references.products}
      />
    </CrmEntityFormShell>
  );
}
