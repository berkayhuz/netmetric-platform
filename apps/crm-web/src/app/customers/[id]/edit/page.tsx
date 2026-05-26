import { notFound } from "next/navigation";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getCustomerDetailData } from "@/features/customers/data/customers-data";
import { CustomerForm } from "@/features/customers/forms/customer-form";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditCustomerPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  await requireCrmSession(`/customers/${resolved.id}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let customer;

  try {
    customer = await getCustomerDetailData(resolved.id, `/customers/${resolved.id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/customers/${resolved.id}/edit`);
  }

  return (
    <CrmEntityFormShell routePath="/customers/[id]/edit" locale={locale}>
      <CustomerForm
        mode="edit"
        customerId={resolved.id}
        initialValues={{
          firstName: customer.firstName,
          lastName: customer.lastName,
          title: customer.title ?? "",
          email: customer.email ?? "",
          mobilePhone: customer.mobilePhone ?? "",
          workPhone: customer.workPhone ?? "",
          personalPhone: customer.personalPhone ?? "",
          birthDate: customer.birthDate?.slice(0, 10) ?? "",
          gender: Number(customer.gender),
          department: customer.department ?? "",
          jobTitle: customer.jobTitle ?? "",
          description: customer.description ?? "",
          notes: customer.notes ?? "",
          ownerUserId: customer.ownerUserId ?? "",
          customerType: Number(customer.customerType),
          identityNumber: customer.identityNumber ?? "",
          isVip: customer.isVip,
          isActive: customer.isActive,
          companyId: customer.companyId ?? "",
          rowVersion: customer.rowVersion,
        }}
        companyOptions={references.companies}
        ownerUserOptions={references.ownerUsers}
      />
    </CrmEntityFormShell>
  );
}
