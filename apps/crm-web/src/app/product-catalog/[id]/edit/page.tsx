import { notFound, redirect } from "next/navigation";
import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import {
  getProductCatalogItemDetail,
  getProductCatalogItemImages,
  getProductCatalogLookups,
  toProductCatalogLookupOptions,
} from "@/features/product-catalog/data/product-catalog-data";
import { ProductCatalogForm } from "@/features/product-catalog/forms/product-catalog-form";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function EditProductCatalogItemPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  await requireCrmSession(`/product-catalog/${id}/edit`);
  const locale = await getRequestLocale();

  if (!isGuid(id)) {
    notFound();
  }

  let detail: Awaited<ReturnType<typeof getProductCatalogItemDetail>> | null = null;
  try {
    detail = await getProductCatalogItemDetail(id, `/product-catalog/${id}/edit`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/product-catalog/${id}/edit`);
  }

  const lookups = await getProductCatalogLookups(`/product-catalog/${id}/edit`);
  const images = await getProductCatalogItemImages(id, `/product-catalog/${id}/edit`);

  if (!detail?.item) {
    if (detail?.forbidden) {
      redirect("/access-denied");
    }

    return (
      <CrmEntityFormShell routePath="/product-catalog/[id]/edit" locale={locale}>
        <Alert>
          <AlertTitle>{tCrm("crm.statusPages.serviceUnavailable.alertTitle", locale)}</AlertTitle>
          <AlertDescription>
            {tCrm("crm.statusPages.serviceUnavailable.alertDescription", locale)}
          </AlertDescription>
        </Alert>
      </CrmEntityFormShell>
    );
  }

  if (lookups.forbidden) {
    redirect("/access-denied");
  }

  const product = detail.item;

  return (
    <CrmEntityFormShell routePath="/product-catalog/[id]/edit" locale={locale}>
      {lookups.unavailable ? (
        <Alert>
          <AlertTitle>{tCrm("crm.statusPages.serviceUnavailable.alertTitle", locale)}</AlertTitle>
          <AlertDescription>
            {tCrm("crm.statusPages.serviceUnavailable.alertDescription", locale)}
          </AlertDescription>
        </Alert>
      ) : null}
      <ProductCatalogForm
        mode="edit"
        productId={id}
        initialValues={{
          code: product.code,
          name: product.name,
          description: product.description ?? "",
          isActive: product.isActive,
          categoryId: product.categoryId ?? undefined,
          unitPrice: product.unitPrice ?? undefined,
          currencyCode: product.currencyCode,
          defaultDiscountRate: product.defaultDiscountRate ?? 0,
          defaultTaxRate: product.defaultTaxRate ?? 0,
        }}
        initialImages={images}
        categoryOptions={toProductCatalogLookupOptions(lookups.categories)}
        currencyOptions={lookups.currencies}
      />
    </CrmEntityFormShell>
  );
}
