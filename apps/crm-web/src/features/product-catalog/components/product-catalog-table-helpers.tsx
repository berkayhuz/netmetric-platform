"use client";

import { type ReactNode } from "react";
import { Button } from "@netmetric/ui";
import { RefreshCw } from "lucide-react";

import type {
  CrmRecordsTableFilter,
  CrmRecordsTableLabels,
} from "@/components/shell/crm-records-table";
import type { ProductCatalogTableLabels } from "./product-catalog-data-table";

export function buildCatalogRecordsTableLabels(
  labels: ProductCatalogTableLabels,
  unavailable: boolean,
): CrmRecordsTableLabels {
  return {
    searchPlaceholder: labels.searchPlaceholder,
    searchAriaLabel: labels.search,
    clear: labels.reset,
    selected: labels.selectedRows.replace("{count}", "").trim() || "selected",
    emptyTitle: unavailable ? labels.errorTitle : labels.emptyTitle,
    emptyDescription: unavailable ? labels.errorDescription : labels.emptyDescription,
  };
}

export function buildActiveInactiveFilter(
  labels: ProductCatalogTableLabels,
): CrmRecordsTableFilter {
  return {
    key: "isActive",
    label: labels.status,
    allLabel: `${labels.status}: ${labels.reset}`,
    options: [
      { label: labels.active, value: labels.active },
      { label: labels.inactive, value: labels.inactive },
    ],
  };
}

export function CatalogRefreshButton({
  label,
  size,
  onRefresh,
}: Readonly<{
  label: string;
  size: "xs" | "sm";
  onRefresh: () => void;
}>) {
  return (
    <Button type="button" variant="outline" size={size} onClick={onRefresh}>
      <RefreshCw aria-hidden="true" />
      {label}
    </Button>
  );
}

export function ProductCatalogActionRow({ children }: Readonly<{ children: ReactNode }>) {
  return <div className="flex flex-wrap items-center justify-end gap-2">{children}</div>;
}
