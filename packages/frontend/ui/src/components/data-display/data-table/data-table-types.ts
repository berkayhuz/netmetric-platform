import type {
  ColumnDef,
  ColumnOrderState,
  ColumnFiltersState,
  PaginationState,
  Row,
  RowSelectionState,
  SortingState,
  Table as TanstackTable,
  Updater,
  VisibilityState,
} from "@tanstack/react-table";
import type * as React from "react";

export type DataTableMode = "client" | "server";

export type DataTableColumnDef<TData, TValue = unknown> = ColumnDef<TData, TValue>;
export type DataTableColumnFiltersState = ColumnFiltersState;
export type DataTablePaginationState = PaginationState;
export type DataTableRowSelectionState = RowSelectionState;
export type DataTableSortingState = SortingState;
export type DataTableUpdater<TValue> = Updater<TValue>;
export type DataTableVisibilityState = VisibilityState;
export type DataTableColumnOrderState = ColumnOrderState;

export interface DataTableColumnMeta {
  label?: string;
  className?: string;
  headerClassName?: string;
  disableReorder?: boolean;
}

export interface DataTableFacetedFilterOption {
  label: string;
  value: string;
  count?: number;
  disabled?: boolean;
  icon?: React.ComponentType<{ className?: string }>;
}

export interface DataTableFacetedFilter {
  columnId: string;
  title: string;
  options: DataTableFacetedFilterOption[];
  multiple?: boolean;
  emptyLabel?: string;
  clearLabel?: string;
}

export interface DataTableLabels {
  actions?: string;
  columns?: string;
  emptyDescription?: string;
  emptyTitle?: string;
  errorDescription?: string;
  errorTitle?: string;
  firstPage?: string;
  lastPage?: string;
  nextPage?: string;
  previousPage?: string;
  page?: string;
  reset?: string;
  rowsPerPage?: string;
  search?: string;
  searchPlaceholder?: string;
  selectedRows?: string;
  viewColumns?: string;
}

export interface DataTableStateContent {
  title?: string;
  description?: string;
  action?: React.ReactNode;
  icon?: React.ReactNode;
}

export interface DataTableToolbarContext<TData> {
  table: TanstackTable<TData>;
  globalFilter: string;
  selectedCount: number;
  selectedRows: Row<TData>[];
}

export interface DataTableRenderContext<TData> {
  table: TanstackTable<TData>;
}

export interface DataTableProps<TData> {
  data: TData[];
  columns: DataTableColumnDef<TData>[];
  getRowId?: (originalRow: TData, index: number, parent?: Row<TData>) => string;
  mode?: DataTableMode;
  totalRows?: number;
  loading?: boolean;
  error?: unknown;
  className?: string;
  caption?: string;
  labels?: DataTableLabels;
  emptyState?: DataTableStateContent;
  errorState?: DataTableStateContent;
  globalFilterPlaceholder?: string;
  enableGlobalFilter?: boolean;
  enableColumnFilters?: boolean;
  enableSorting?: boolean;
  enablePagination?: boolean;
  enableRowSelection?: boolean;
  enableColumnVisibility?: boolean;
  enableColumnReorder?: boolean;
  facetedFilters?: DataTableFacetedFilter[];
  pageSizeOptions?: number[];
  skeletonRows?: number;
  sorting?: SortingState;
  columnFilters?: ColumnFiltersState;
  globalFilter?: string;
  pagination?: PaginationState;
  rowSelection?: RowSelectionState;
  columnVisibility?: VisibilityState;
  columnOrder?: ColumnOrderState;
  initialColumnVisibility?: VisibilityState;
  initialColumnOrder?: ColumnOrderState;
  onSortingChange?: (value: SortingState) => void;
  onColumnFiltersChange?: (value: ColumnFiltersState) => void;
  onGlobalFilterChange?: (value: string) => void;
  onPaginationChange?: (value: PaginationState) => void;
  onRowSelectionChange?: (value: RowSelectionState) => void;
  onColumnVisibilityChange?: (value: VisibilityState) => void;
  onColumnOrderChange?: (value: ColumnOrderState) => void;
  toolbarActions?: React.ReactNode | ((context: DataTableToolbarContext<TData>) => React.ReactNode);
  renderToolbar?: (context: DataTableToolbarContext<TData>) => React.ReactNode;
  renderRowActions?: (row: Row<TData>) => React.ReactNode;
  renderBulkActions?: (rows: Row<TData>[]) => React.ReactNode;
  renderEmpty?: (context: DataTableRenderContext<TData>) => React.ReactNode;
  renderLoading?: (context: DataTableRenderContext<TData>) => React.ReactNode;
  renderError?: (context: DataTableRenderContext<TData> & { error: string }) => React.ReactNode;
}
