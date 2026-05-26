import { redirect } from "next/navigation";
import Link from "next/link";
import { Alert, AlertDescription, AlertTitle, Button } from "@netmetric/ui";
import { Plus, Tags } from "lucide-react";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { ProductCatalogDataTable } from "@/features/product-catalog/components/product-catalog-data-table";
import {
  getProductCatalogData,
  getProductCatalogLookups,
  getProductCatalogMeta,
  getProductCatalogStats,
  toProductCatalogListQuery,
} from "@/features/product-catalog/data/product-catalog-data";
import { getProductCatalogTableLabels } from "@/features/product-catalog/data/product-catalog-table-labels";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function ProductCatalogPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/product-catalog");
  const locale = await getRequestLocale();

  const params = await searchParams;
  const query = toProductCatalogListQuery(params);
  const [data, lookups, meta, stats] = await Promise.all([
    getProductCatalogData(query, "/product-catalog"),
    getProductCatalogLookups("/product-catalog"),
    getProductCatalogMeta("/product-catalog"),
    getProductCatalogStats("/product-catalog"),
  ]);

  if (data.forbidden || lookups.forbidden || meta.forbidden || stats.forbidden) {
    redirect("/access-denied");
  }

  const canCreate = crmCapabilityAllows(session.capabilities, "productCatalog.manage");

  return (
    <CrmPageShell
      routePath="/product-catalog"
      locale={locale}
      actions={
        <div className="flex flex-wrap gap-2">
          <Button asChild variant="outline">
            <Link href="/product-catalog/categories">
              <Tags aria-hidden="true" />
              {tCrm("crm.productCatalog.categories.actions.manage", locale)}
            </Link>
          </Button>
          {canCreate ? (
            <Button asChild>
              <Link href="/product-catalog/new">
                <Plus aria-hidden="true" />
                {tCrm("crm.productCatalog.actions.create", locale)}
              </Link>
            </Button>
          ) : null}
        </div>
      }
    >
      {lookups.unavailable ? (
        <Alert>
          <AlertTitle>{tCrm("crm.statusPages.serviceUnavailable.alertTitle", locale)}</AlertTitle>
          <AlertDescription>
            {tCrm("crm.productCatalog.categories.states.lookupUnavailable", locale)}
          </AlertDescription>
        </Alert>
      ) : null}

      <ProductCatalogDataTable
        paged={data.paged}
        categories={lookups.categories}
        locale={locale}
        labels={getProductCatalogTableLabels(locale, "products")}
        unavailable={data.unavailable}
        canManage={canCreate}
        activeProductCount={stats.stats?.activeProductCount}
        categoryCount={stats.stats?.categoryCount}
        version={meta.meta?.version}
      />
    </CrmPageShell>
  );
}
