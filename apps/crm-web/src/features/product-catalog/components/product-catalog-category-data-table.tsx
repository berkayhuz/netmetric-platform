"use client";

import { useMemo } from "react";
import { useRouter } from "next/navigation";

import {
  CrmRecordsTable,
  type CrmRecordsTableFilter,
  type CrmRecordsTableRow,
} from "@/components/shell/crm-records-table";
import type { CrmPagedResult, ProductCatalogCategoryDto } from "@/lib/crm-api";
import type { ProductCatalogTableLabels } from "./product-catalog-data-table";
import {
  buildActiveInactiveFilter,
  buildCatalogRecordsTableLabels,
  CatalogRefreshButton,
} from "./product-catalog-table-helpers";

export function ProductCatalogCategoryDataTable({
  paged,
  labels,
  unavailable,
}: Readonly<{
  paged: CrmPagedResult<ProductCatalogCategoryDto>;
  labels: ProductCatalogTableLabels;
  unavailable: boolean;
}>) {
  const router = useRouter();
  const rows = useMemo<CrmRecordsTableRow[]>(
    () =>
      paged.items.map((category) => {
        const status = category.isActive ? labels.active : labels.inactive;

        return {
          id: category.id,
          href: `/product-catalog/categories/${category.id}`,
          cells: {
            name: category.name,
            code: category.code,
            isActive: status,
          },
          descriptions: {
            name: category.description ?? undefined,
          },
          searchText: [category.name, category.code, category.description, status]
            .filter(Boolean)
            .join(" "),
          filterValues: {
            isActive: status,
          },
        };
      }),
    [labels, paged.items],
  );
  const filters = useMemo<CrmRecordsTableFilter[]>(
    () => [buildActiveInactiveFilter(labels)],
    [labels],
  );

  return (
    <div>
      <CrmRecordsTable
        caption={labels.description}
        columns={[
          { key: "name", header: labels.name },
          { key: "code", header: labels.code },
          { key: "isActive", header: labels.status, badge: true },
        ]}
        rows={unavailable ? [] : rows}
        filters={filters}
        labels={buildCatalogRecordsTableLabels(labels, unavailable)}
        toolbarContent={
          <CatalogRefreshButton
            label={labels.refresh}
            size="xs"
            onRefresh={() => router.refresh()}
          />
        }
      />
    </div>
  );
}
