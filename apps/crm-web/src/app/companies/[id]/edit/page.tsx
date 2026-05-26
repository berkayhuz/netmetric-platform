import { notFound } from "next/navigation";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getCompanyDetailData } from "@/features/companies/data/companies-data";
import { CompanyForm } from "@/features/companies/forms/company-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditCompanyPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  await requireCrmSession(`/companies/${resolved.id}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let company;

  try {
    company = await getCompanyDetailData(resolved.id, `/companies/${resolved.id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/companies/${resolved.id}/edit`);
  }

  return (
    <CrmEntityFormShell routePath="/companies/[id]/edit" locale={locale}>
      <CompanyForm
        mode="edit"
        companyId={resolved.id}
        initialValues={{
          name: company.name,
          taxNumber: company.taxNumber ?? "",
          taxOffice: company.taxOffice ?? "",
          website: company.website ?? "",
          email: company.email ?? "",
          phone: company.phone ?? "",
          sector: company.sector ?? "",
          employeeCountRange: company.employeeCountRange ?? "",
          annualRevenue: company.annualRevenue ?? undefined,
          description: company.description ?? "",
          notes: company.notes ?? "",
          companyType: Number(company.companyType),
          ownerUserId: company.ownerUserId ?? "",
          parentCompanyId: company.parentCompanyId ?? "",
          rowVersion: company.rowVersion,
        }}
        companyOptions={references.companies}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
