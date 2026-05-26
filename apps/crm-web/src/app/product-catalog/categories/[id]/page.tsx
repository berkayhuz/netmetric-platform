import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { Alert, AlertDescription, AlertTitle, Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmEntityDetailPanel } from "@/components/shell/crm-entity-detail-panel";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { deleteProductCatalogCategoryAction } from "@/features/product-catalog/actions/product-catalog-mutation-actions";
import { getProductCatalogCategoryDetail } from "@/features/product-catalog/data/product-catalog-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function ProductCatalogCategoryDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const session = await requireCrmSession(`/product-catalog/categories/${id}`);
  const locale = await getRequestLocale();
  const canManage = crmCapabilityAllows(session.capabilities, "productCatalog.manage");

  if (!isGuid(id)) {
    notFound();
  }

  let detail: Awaited<ReturnType<typeof getProductCatalogCategoryDetail>> | null = null;
  try {
    detail = await getProductCatalogCategoryDetail(id, `/product-catalog/categories/${id}`);
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/product-catalog/categories/${id}`);
  }

  if (!detail?.item) {
    if (detail?.forbidden) {
      redirect("/access-denied");
    }

    return (
      <CrmPageShell
        title={tCrm("crm.productCatalog.categories.pages.detail.profileTitle", locale)}
        description={tCrm("crm.productCatalog.categories.pages.detail.description", locale)}
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

  const category = detail.item;

  return (
    <CrmPageShell
      title={category.name}
      description={tCrm("crm.productCatalog.categories.pages.detail.description", locale)}
      actions={
        canManage ? (
          <Button asChild>
            <Link href={`/product-catalog/categories/${id}/edit`}>
              {tCrm("crm.productCatalog.categories.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <CrmEntityDetailPanel
        title={tCrm("crm.productCatalog.categories.pages.detail.profileTitle", locale)}
        fields={[
          { label: tCrm("crm.productCatalog.fields.code", locale), value: category.code },
          { label: tCrm("crm.productCatalog.fields.name", locale), value: category.name },
          {
            label: tCrm("crm.productCatalog.fields.description", locale),
            value: category.description,
          },
          {
            label: tCrm("crm.productCatalog.fields.status", locale),
            value: category.isActive
              ? tCrm("crm.common.active", locale)
              : tCrm("crm.common.inactive", locale),
          },
        ]}
      />

      {canManage ? (
        <CrmDeleteZone
          title={tCrm("crm.productCatalog.categories.actions.delete", locale)}
          description={tCrm("crm.productCatalog.categories.pages.detail.deleteDescription", locale)}
          locale={locale}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.productCatalog.categories.entityName", locale)}
            entityName={category.name}
            confirmValue="delete-product-catalog-category"
            action={deleteProductCatalogCategoryAction.bind(null, id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
