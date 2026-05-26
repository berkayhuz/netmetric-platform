import { notFound, redirect } from "next/navigation";
import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getProductCatalogCategoryDetail } from "@/features/product-catalog/data/product-catalog-data";
import { ProductCatalogCategoryForm } from "@/features/product-catalog/forms/product-catalog-category-form";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditProductCatalogCategoryPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  await requireCrmSession(`/product-catalog/categories/${id}/edit`);
  const locale = await getRequestLocale();

  if (!isGuid(id)) {
    notFound();
  }

  let detail: Awaited<ReturnType<typeof getProductCatalogCategoryDetail>> | null = null;
  try {
    detail = await getProductCatalogCategoryDetail(id, `/product-catalog/categories/${id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/product-catalog/categories/${id}/edit`);
  }

  if (!detail?.item) {
    if (detail?.forbidden) {
      redirect("/access-denied");
    }

    return (
      <CrmEntityFormShell routePath="/product-catalog/categories/[id]/edit" locale={locale}>
        <Alert>
          <AlertTitle>{tCrm("crm.statusPages.serviceUnavailable.alertTitle", locale)}</AlertTitle>
          <AlertDescription>
            {tCrm("crm.statusPages.serviceUnavailable.alertDescription", locale)}
          </AlertDescription>
        </Alert>
      </CrmEntityFormShell>
    );
  }

  const category = detail.item;

  return (
    <CrmEntityFormShell routePath="/product-catalog/categories/[id]/edit" locale={locale}>
      <ProductCatalogCategoryForm
        mode="edit"
        categoryId={id}
        initialValues={{
          code: category.code,
          name: category.name,
          description: category.description ?? "",
          isActive: category.isActive,
        }}
        initialImageUrl={category.primaryImageUrl ?? null}
      />
    </CrmEntityFormShell>
  );
}
