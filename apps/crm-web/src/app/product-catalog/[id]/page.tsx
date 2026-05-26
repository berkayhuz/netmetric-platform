import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { Alert, AlertDescription, AlertTitle, Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmEntityDetailPanel } from "@/components/shell/crm-entity-detail-panel";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { deleteProductCatalogItemAction } from "@/features/product-catalog/actions/product-catalog-mutation-actions";
import { getProductCatalogItemDetail } from "@/features/product-catalog/data/product-catalog-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function ProductCatalogDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const session = await requireCrmSession(`/product-catalog/${id}`);
  const locale = await getRequestLocale();
  const canManage = crmCapabilityAllows(session.capabilities, "productCatalog.manage");

  if (!isGuid(id)) {
    notFound();
  }

  let detail: Awaited<ReturnType<typeof getProductCatalogItemDetail>> | null = null;
  try {
    detail = await getProductCatalogItemDetail(id, `/product-catalog/${id}`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/product-catalog/${id}`);
  }

  if (!detail?.item) {
    if (detail?.forbidden) {
      redirect("/access-denied");
    }

    return (
      <CrmPageShell
        title={tCrm("crm.productCatalog.pages.detail.profileTitle", locale)}
        description={tCrm("crm.productCatalog.pages.detail.description", locale)}
      >
        <Alert>
          <AlertTitle>{tCrm("crm.statusPages.serviceUnavailable.alertTitle", locale)}</AlertTitle>
          <AlertDescription>
            {tCrm("crm.statusPages.serviceUnavailable.alertDescription", locale)}
          </AlertDescription>
        </Alert>
      </CrmPageShell>
    );
  }

  const product = detail.item;

  return (
    <CrmPageShell
      title={product.name}
      description={tCrm("crm.productCatalog.pages.detail.description", locale)}
      actions={
        canManage ? (
          <Button asChild>
            <Link href={`/product-catalog/${id}/edit`}>
              {tCrm("crm.productCatalog.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <CrmEntityDetailPanel
        title={tCrm("crm.productCatalog.pages.detail.profileTitle", locale)}
        fields={[
          { label: tCrm("crm.productCatalog.fields.code", locale), value: product.code },
          { label: tCrm("crm.productCatalog.fields.name", locale), value: product.name },
          {
            label: tCrm("crm.productCatalog.fields.description", locale),
            value: product.description,
          },
          {
            label: tCrm("crm.productCatalog.fields.category", locale),
            value: product.categoryName,
          },
          {
            label: tCrm("crm.productCatalog.fields.price", locale),
            value:
              product.unitPrice == null
                ? "-"
                : `${product.unitPrice.toFixed(2)} ${product.currencyCode}`,
          },
          {
            label: tCrm("crm.productCatalog.fields.currencyCode", locale),
            value: product.currencyCode,
          },
          {
            label: tCrm("crm.quotes.fields.discountRate", locale),
            value: `${product.defaultDiscountRate ?? 0}%`,
          },
          {
            label: tCrm("crm.quotes.fields.taxRate", locale),
            value: `${product.defaultTaxRate ?? 0}%`,
          },
          {
            label: tCrm("crm.productCatalog.fields.status", locale),
            value: product.isActive
              ? tCrm("crm.common.active", locale)
              : tCrm("crm.common.inactive", locale),
          },
        ]}
      />

      {canManage ? (
        <CrmDeleteZone
          title={tCrm("crm.productCatalog.actions.delete", locale)}
          description={tCrm("crm.productCatalog.pages.detail.deleteDescription", locale)}
          locale={locale}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.productCatalog.entityName", locale)}
            entityName={product.name}
            confirmValue="delete-product-catalog-item"
            action={deleteProductCatalogItemAction.bind(null, id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
