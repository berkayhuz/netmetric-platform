import { redirect } from "next/navigation";
import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { ProductCatalogForm } from "@/features/product-catalog/forms/product-catalog-form";
import {
  getProductCatalogLookups,
  toProductCatalogLookupOptions,
} from "@/features/product-catalog/data/product-catalog-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewProductCatalogItemPage() {
  await requireCrmSession("/product-catalog/new");
  const locale = await getRequestLocale();
  const lookups = await getProductCatalogLookups("/product-catalog/new");
  if (lookups.forbidden) {
    redirect("/access-denied");
  }

  return (
    <CrmEntityFormShell routePath="/product-catalog/new" locale={locale}>
      {lookups.unavailable ? (
        <Alert>
          <AlertTitle>{tCrm("crm.statusPages.serviceUnavailable.alertTitle", locale)}</AlertTitle>
          <AlertDescription>
            {tCrm("crm.statusPages.serviceUnavailable.alertDescription", locale)}
          </AlertDescription>
        </Alert>
      ) : null}
      <ProductCatalogForm
        mode="create"
        categoryOptions={toProductCatalogLookupOptions(lookups.categories)}
        currencyOptions={lookups.currencies}
      />
    </CrmEntityFormShell>
  );
}
