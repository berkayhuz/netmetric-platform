import type {
  ColumnDef,
  ColumnFiltersState,
  PaginationState,
  Row,
  RowSelectionState,
  SortingState,
  Table as TanstackTable,
  VisibilityState,
} from "@tanstack/react-table";
import type * as React from "react";

export type DataGridMode = "client" | "server";

export interface DataGridToolbarContext<TData> {
  table: TanstackTable<TData>;
  globalFilter: string;
  selectedCount: number;
}

export interface DataGridRenderContext<TData> {
  table: TanstackTable<TData>;
}

export interface DataGridPaginationContext {
  pageIndex: number;
  pageSize: number;
  totalRows: number;
  pageCount: number;
}

export interface DataGridProps<TData> {
  data: TData[];
  columns: ColumnDef<TData, unknown>[];
  getRowId?: (originalRow: TData, index: number, parent?: Row<TData>) => string;
  mode?: DataGridMode;
  totalRows?: number;
  loading?: boolean;
  error?: string | null;
  globalFilterPlaceholder?: string;
  enableGlobalFilter?: boolean;
  enableColumnFilters?: boolean;
  enableSorting?: boolean;
  enableRowSelection?: boolean;
  enableColumnVisibility?: boolean;
  sorting?: SortingState;
  columnFilters?: ColumnFiltersState;
  globalFilter?: string;
  pagination?: PaginationState;
  rowSelection?: RowSelectionState;
  columnVisibility?: VisibilityState;
  onSortingChange?: (value: SortingState) => void;
  onColumnFiltersChange?: (value: ColumnFiltersState) => void;
  onGlobalFilterChange?: (value: string) => void;
  onPaginationChange?: (value: PaginationState) => void;
  onRowSelectionChange?: (value: RowSelectionState) => void;
  onColumnVisibilityChange?: (value: VisibilityState) => void;
  renderToolbar?: (context: DataGridToolbarContext<TData>) => React.ReactNode;
  renderRowActions?: (row: Row<TData>) => React.ReactNode;
  renderBulkActions?: (rows: Row<TData>[]) => React.ReactNode;
  renderEmpty?: (context: DataGridRenderContext<TData>) => React.ReactNode;
  renderLoading?: (context: DataGridRenderContext<TData>) => React.ReactNode;
  renderError?: (context: DataGridRenderContext<TData> & { error: string }) => React.ReactNode;
}
