import { redirect } from "next/navigation";
import Link from "next/link";
import { Button } from "@netmetric/ui";
import { Plus } from "lucide-react";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { ProductCatalogCategoryDataTable } from "@/features/product-catalog/components/product-catalog-category-data-table";
import {
  getProductCatalogCategoriesData,
  toProductCatalogCategoryListQuery,
} from "@/features/product-catalog/data/product-catalog-data";
import { getProductCatalogTableLabels } from "@/features/product-catalog/data/product-catalog-table-labels";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function ProductCatalogCategoriesPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/product-catalog/categories");
  const locale = await getRequestLocale();
  const params = await searchParams;
  const query = toProductCatalogCategoryListQuery(params);
  const data = await getProductCatalogCategoriesData(query, "/product-catalog/categories");
  const canManage = crmCapabilityAllows(session.capabilities, "productCatalog.manage");

  if (data.forbidden) {
    redirect("/access-denied");
  }

  return (
    <CrmPageShell
      routePath="/product-catalog/categories"
      locale={locale}
      actions={
        canManage ? (
          <Button asChild>
            <Link href="/product-catalog/categories/new">
              <Plus aria-hidden="true" />
              {tCrm("crm.productCatalog.categories.actions.create", locale)}
            </Link>
          </Button>
        ) : null
      }
    >
      <ProductCatalogCategoryDataTable
        paged={data.paged}
        labels={getProductCatalogTableLabels(locale, "categories")}
        unavailable={data.unavailable}
      />
    </CrmPageShell>
  );
}
