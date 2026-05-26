"use client";

import type {
  CrmRecordsTableColumn,
  CrmRecordsTableRow,
} from "@/components/shell/crm-records-table";
import type { ProductCatalogItemDto } from "@/lib/crm-api";

import type { ProductCatalogTableLabels } from "./product-catalog-data-table";

export function formatProductCatalogCurrency(
  value: number | null | undefined,
  currencyCode: string,
  locale: string | null,
): string {
  if (value == null) {
    return "";
  }

  try {
    return new Intl.NumberFormat(locale ?? undefined, {
      style: "currency",
      currency: currencyCode,
      maximumFractionDigits: 2,
    }).format(value);
  } catch {
    return `${value.toFixed(2)} ${currencyCode}`;
  }
}

export function buildProductCatalogColumns(
  labels: ProductCatalogTableLabels,
): CrmRecordsTableColumn[] {
  return [
    { key: "name", header: labels.name },
    { key: "code", header: labels.code },
    { key: "categoryName", header: labels.category, badge: true },
    { key: "unitPrice", header: labels.price },
    { key: "isActive", header: labels.status, badge: true },
  ];
}

export function mapProductCatalogRows(params: {
  items: ProductCatalogItemDto[];
  labels: ProductCatalogTableLabels;
  locale: string | null;
}): CrmRecordsTableRow[] {
  const { items, labels, locale } = params;

  return items.map((item) => {
    const price = formatProductCatalogCurrency(item.unitPrice, item.currencyCode, locale);
    const category = item.categoryName ?? labels.noCategory;
    const status = item.isActive ? labels.active : labels.inactive;

    return {
      id: item.id,
      href: `/product-catalog/${item.id}`,
      cells: {
        name: item.name,
        code: item.code,
        categoryName: category,
        unitPrice: price || labels.noPrice,
        isActive: status,
      },
      descriptions: {
        name: item.description ?? undefined,
        categoryName: item.categoryCode ?? undefined,
      },
      searchText: [
        item.name,
        item.code,
        item.description,
        item.categoryName,
        item.categoryCode,
        price,
        status,
      ]
        .filter(Boolean)
        .join(" "),
      filterValues: {
        categoryName: category,
        isActive: status,
      },
    };
  });
}
