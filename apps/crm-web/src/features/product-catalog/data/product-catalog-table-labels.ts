import { tCrm } from "@/lib/i18n/crm-i18n";

import type { ProductCatalogTableLabels } from "../components/product-catalog-data-table";

export function getProductCatalogTableLabels(
  locale: string | null,
  scope: "products" | "categories",
): ProductCatalogTableLabels {
  return {
    actions: tCrm("crm.common.actions", locale),
    active: tCrm("crm.common.active", locale),
    addProduct:
      scope === "products"
        ? tCrm("crm.productCatalog.actions.new", locale)
        : tCrm("crm.productCatalog.categories.actions.new", locale),
    category: tCrm("crm.productCatalog.fields.category", locale),
    code: tCrm("crm.productCatalog.fields.code", locale),
    columns: tCrm("crm.tables.columns", locale),
    description:
      scope === "products"
        ? tCrm("crm.productCatalog.pages.list.caption", locale)
        : tCrm("crm.productCatalog.categories.pages.list.caption", locale),
    edit:
      scope === "products"
        ? tCrm("crm.productCatalog.actions.edit", locale)
        : tCrm("crm.productCatalog.categories.actions.edit", locale),
    emptyDescription:
      scope === "products"
        ? tCrm("crm.productCatalog.states.emptyDescription", locale)
        : tCrm("crm.productCatalog.categories.states.emptyDescription", locale),
    emptyTitle:
      scope === "products"
        ? tCrm("crm.productCatalog.states.emptyTitle", locale)
        : tCrm("crm.productCatalog.categories.states.emptyTitle", locale),
    errorDescription: tCrm("crm.statusPages.serviceUnavailable.alertDescription", locale),
    errorTitle: tCrm("crm.statusPages.serviceUnavailable.alertTitle", locale),
    firstPage: tCrm("crm.tables.firstPage", locale),
    inactive: tCrm("crm.common.inactive", locale),
    lastPage: tCrm("crm.tables.lastPage", locale),
    manageCategories: tCrm("crm.productCatalog.categories.actions.manage", locale),
    name: tCrm("crm.productCatalog.fields.name", locale),
    nextPage: tCrm("crm.tables.nextPage", locale),
    noCategory: tCrm("crm.productCatalog.states.noCategory", locale),
    noPrice: tCrm("crm.productCatalog.states.noPrice", locale),
    page: tCrm("crm.tables.page", locale),
    previousPage: tCrm("crm.tables.previousPage", locale),
    price: tCrm("crm.productCatalog.fields.price", locale),
    refresh: tCrm("crm.common.refresh", locale),
    reset: tCrm("crm.lists.toolbar.clear", locale),
    rowsPerPage: tCrm("crm.tables.rowsPerPage", locale),
    search: tCrm("crm.lists.toolbar.search", locale),
    searchPlaceholder:
      scope === "products"
        ? tCrm("crm.productCatalog.filters.searchPlaceholder", locale)
        : tCrm("crm.productCatalog.categories.filters.searchPlaceholder", locale),
    selectedRows: tCrm("crm.tables.selectedRows", locale),
    status: tCrm("crm.productCatalog.fields.status", locale),
    view:
      scope === "products"
        ? tCrm("crm.productCatalog.actions.view", locale)
        : tCrm("crm.productCatalog.categories.actions.view", locale),
    viewColumns: tCrm("crm.tables.viewColumns", locale),
  };
}
