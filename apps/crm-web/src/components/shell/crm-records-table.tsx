"use client";

import { type ReactNode } from "react";
import type { DataTableStateContent } from "@netmetric/ui/client";

import { CrmDataTableAdapter } from "./crm-data-table-adapter";

export type CrmRecordsTableColumn = {
  key: string;
  header: string;
  sortable?: boolean;
  badge?: boolean;
  className?: string;
  render?: (row: CrmRecordsTableRow) => ReactNode;
};

export type CrmRecordsTableRow = {
  id: string;
  href?: string;
  cells: Record<string, string>;
  descriptions?: Record<string, string | undefined>;
  searchText?: string;
  sortValues?: Record<string, string | number | undefined>;
  filterValues?: Record<string, string | undefined>;
};

export type CrmRecordsTableFilter = {
  key: string;
  label: string;
  allLabel: string;
  options: Array<{
    value: string;
    label: string;
  }>;
};

export type CrmRecordsTableLabels = {
  searchPlaceholder?: string;
  searchAriaLabel?: string;
  clear?: string;
  shown?: string;
  selected?: string;
  selectAll?: string;
  selectRow?: (rowLabel: string) => string;
  emptyTitle?: string;
  emptyDescription?: string;
};

export function CrmRecordsTable({
  caption,
  columns,
  rows,
  filters = [],
  initialSearch = "",
  labels = {},
  minWidthClassName = "min-w-[860px]",
  toolbarContent,
  filterBarLeadingContent,
  infoContent,
  onSelectionChange,
  emptyState,
  selectionActions,
  enableSelection,
  rowActions,
}: Readonly<{
  caption: string;
  columns: CrmRecordsTableColumn[];
  rows: CrmRecordsTableRow[];
  filters?: CrmRecordsTableFilter[];
  initialSearch?: string;
  labels?: CrmRecordsTableLabels;
  minWidthClassName?: string;
  toolbarContent?: ReactNode;
  filterBarLeadingContent?: ReactNode;
  infoContent?: ReactNode;
  onSelectionChange?: (selectedIds: string[]) => void;
  emptyState?: DataTableStateContent;
  selectionActions?: ReactNode | ((context: { selectedCount: number }) => ReactNode);
  enableSelection?: boolean;
  rowActions?: (row: CrmRecordsTableRow) => ReactNode;
}>) {
  return (
    <CrmDataTableAdapter
      caption={caption}
      columns={columns}
      rows={rows}
      filters={filters}
      initialSearch={initialSearch}
      labels={labels}
      minWidthClassName={minWidthClassName}
      toolbarContent={toolbarContent}
      filterBarLeadingContent={filterBarLeadingContent}
      infoContent={infoContent}
      {...(emptyState ? { emptyState } : {})}
      {...(selectionActions ? { selectionActions } : {})}
      {...(enableSelection !== undefined ? { enableSelection } : {})}
      {...(rowActions ? { rowActions } : {})}
      {...(onSelectionChange ? { onSelectionChange } : {})}
    />
  );
}
