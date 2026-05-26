import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { ProductCatalogCategoryForm } from "@/features/product-catalog/forms/product-catalog-category-form";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewProductCatalogCategoryPage() {
  await requireCrmSession("/product-catalog/categories/new");
  const locale = await getRequestLocale();

  return (
    <CrmEntityFormShell routePath="/product-catalog/categories/new" locale={locale}>
      <ProductCatalogCategoryForm mode="create" />
    </CrmEntityFormShell>
  );
}
