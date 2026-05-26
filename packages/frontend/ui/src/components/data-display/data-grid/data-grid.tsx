"use client";

import {
  functionalUpdate,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
  type ColumnDef,
  type ColumnFiltersState,
  type PaginationState,
  type RowSelectionState,
  type SortingState,
  type Updater,
  type VisibilityState,
} from "@tanstack/react-table";
import * as React from "react";

import { Checkbox } from "../../primitives/checkbox";

import { getPaginationDefaults, resolveTotalRows, toErrorMessage } from "./data-grid-internal";
import { DataGridPagination } from "./data-grid-pagination";
import { DataGridTable } from "./data-grid-table";
import { DataGridToolbar } from "./data-grid-toolbar";

import type { DataGridProps } from "./data-grid-types";

function updateState<T>(updater: Updater<T>, current: T): T {
  return functionalUpdate(updater, current);
}

function buildControlledStateHandler<T>(
  controlledValue: T | undefined,
  internalValue: T,
  setInternalValue: React.Dispatch<React.SetStateAction<T>>,
  onChange?: (value: T) => void,
): (updater: Updater<T>) => void {
  return (updater) => {
    const next = updateState(updater, controlledValue ?? internalValue);
    if (controlledValue === undefined) {
      setInternalValue(next);
    }
    onChange?.(next);
  };
}

function createSelectionColumn<TData>(): ColumnDef<TData, unknown> {
  return {
    id: "__select",
    enableHiding: false,
    enableSorting: false,
    header: ({ table }) => (
      <Checkbox
        aria-label="Select all rows on page"
        checked={table.getIsAllPageRowsSelected() || table.getIsSomePageRowsSelected()}
        onCheckedChange={(value) => table.toggleAllPageRowsSelected(Boolean(value))}
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        aria-label={`Select row ${row.id}`}
        checked={row.getIsSelected()}
        disabled={!row.getCanSelect()}
        onCheckedChange={(value) => row.toggleSelected(Boolean(value))}
      />
    ),
  };
}

export function DataGrid<TData>({
  data,
  columns,
  getRowId,
  mode = "client",
  totalRows,
  loading = false,
  error = null,
  globalFilterPlaceholder = "Search...",
  enableGlobalFilter = true,
  enableColumnFilters = true,
  enableSorting = true,
  enableRowSelection = false,
  enableColumnVisibility = true,
  sorting,
  columnFilters,
  globalFilter,
  pagination,
  rowSelection,
  columnVisibility,
  onSortingChange,
  onColumnFiltersChange,
  onGlobalFilterChange,
  onPaginationChange,
  onRowSelectionChange,
  onColumnVisibilityChange,
  renderToolbar,
  renderRowActions,
  renderBulkActions,
  renderEmpty,
  renderLoading,
  renderError,
}: DataGridProps<TData>): React.JSX.Element {
  const isServerMode = mode === "server";
  const hasWarnedMissingTotalRowsRef = React.useRef(false);

  const [sortingState, setSortingState] = React.useState<SortingState>(sorting ?? []);
  const [columnFiltersState, setColumnFiltersState] = React.useState<ColumnFiltersState>(
    columnFilters ?? [],
  );
  const [globalFilterState, setGlobalFilterState] = React.useState<string>(globalFilter ?? "");
  const [paginationState, setPaginationState] = React.useState<PaginationState>(
    getPaginationDefaults(pagination),
  );
  const [rowSelectionState, setRowSelectionState] = React.useState<RowSelectionState>(
    rowSelection ?? {},
  );
  const [columnVisibilityState, setColumnVisibilityState] = React.useState<VisibilityState>(
    columnVisibility ?? {},
  );

  React.useEffect(() => {
    if (sorting) {
      setSortingState(sorting);
    }
  }, [sorting]);

  React.useEffect(() => {
    if (columnFilters) {
      setColumnFiltersState(columnFilters);
    }
  }, [columnFilters]);

  React.useEffect(() => {
    if (globalFilter !== undefined) {
      setGlobalFilterState(globalFilter);
    }
  }, [globalFilter]);

  React.useEffect(() => {
    if (pagination) {
      setPaginationState(getPaginationDefaults(pagination));
    }
  }, [pagination]);

  React.useEffect(() => {
    if (rowSelection) {
      setRowSelectionState(rowSelection);
    }
  }, [rowSelection]);

  React.useEffect(() => {
    if (columnVisibility) {
      setColumnVisibilityState(columnVisibility);
    }
  }, [columnVisibility]);

  const safeColumns = React.useMemo<ColumnDef<TData, unknown>[]>(() => {
    if (!enableRowSelection) {
      return columns;
    }
    return [createSelectionColumn<TData>(), ...columns];
  }, [columns, enableRowSelection]);

  React.useEffect(() => {
    if (!isServerMode || totalRows !== undefined || hasWarnedMissingTotalRowsRef.current) {
      return;
    }
    hasWarnedMissingTotalRowsRef.current = true;
    if (typeof console !== "undefined") {
      console.warn(
        "[@netmetric/ui] DataGrid mode='server' used without totalRows. Falling back to current data length for pagination display.",
      );
    }
  }, [isServerMode, totalRows]);

  const tableOptions = {
    data,
    columns: safeColumns,
    state: {
      sorting: sorting ?? sortingState,
      columnFilters: columnFilters ?? columnFiltersState,
      globalFilter: globalFilter ?? globalFilterState,
      pagination: pagination ?? paginationState,
      rowSelection: rowSelection ?? rowSelectionState,
      columnVisibility: columnVisibility ?? columnVisibilityState,
    },
    manualPagination: isServerMode,
    manualSorting: isServerMode,
    manualFiltering: isServerMode,
    enableSorting,
    enableFilters: enableColumnFilters,
    enableGlobalFilter,
    enableRowSelection,
    enableHiding: enableColumnVisibility,
    getCoreRowModel: getCoreRowModel(),
    ...(!isServerMode
      ? {
          getSortedRowModel: getSortedRowModel(),
          getFilteredRowModel: getFilteredRowModel(),
          getPaginationRowModel: getPaginationRowModel(),
        }
      : {}),
    onSortingChange: buildControlledStateHandler(
      sorting,
      sortingState,
      setSortingState,
      onSortingChange,
    ),
    onColumnFiltersChange: buildControlledStateHandler(
      columnFilters,
      columnFiltersState,
      setColumnFiltersState,
      onColumnFiltersChange,
    ),
    onGlobalFilterChange: buildControlledStateHandler(
      globalFilter,
      globalFilterState,
      setGlobalFilterState,
      onGlobalFilterChange,
    ),
    onPaginationChange: buildControlledStateHandler(
      pagination,
      paginationState,
      setPaginationState,
      onPaginationChange,
    ),
    onRowSelectionChange: buildControlledStateHandler(
      rowSelection,
      rowSelectionState,
      setRowSelectionState,
      onRowSelectionChange,
    ),
    onColumnVisibilityChange: buildControlledStateHandler(
      columnVisibility,
      columnVisibilityState,
      setColumnVisibilityState,
      onColumnVisibilityChange,
    ),
  };

  const table = useReactTable({
    ...tableOptions,
    ...(getRowId ? { getRowId } : {}),
  });

  const resolvedError = toErrorMessage(error);
  const selectedRows = table.getSelectedRowModel().rows;
  const resolvedTotalRows = resolveTotalRows(
    mode,
    totalRows,
    isServerMode ? data.length : table.getFilteredRowModel().rows.length,
  );
  const currentPageSize = table.getState().pagination.pageSize;
  const safePageSize = currentPageSize > 0 ? currentPageSize : 1;
  const isPaginationVisible = resolvedTotalRows > safePageSize;

  return (
    <div className="space-y-4" data-slot="data-grid">
      <DataGridToolbar
        enableGlobalFilter={enableGlobalFilter}
        globalFilter={globalFilter ?? globalFilterState}
        globalFilterPlaceholder={globalFilterPlaceholder}
        selectedRows={selectedRows}
        table={table}
        onGlobalFilterChange={(value) => table.setGlobalFilter(value)}
        {...(renderBulkActions ? { renderBulkActions } : {})}
        {...(renderToolbar ? { renderToolbar } : {})}
      />
      <DataGridTable
        error={resolvedError}
        loading={loading}
        table={table}
        {...(renderEmpty ? { renderEmpty } : {})}
        {...(renderError ? { renderError } : {})}
        {...(renderLoading ? { renderLoading } : {})}
        {...(renderRowActions ? { renderRowActions } : {})}
      />
      {isPaginationVisible ? (
        <DataGridPagination table={table} totalRows={resolvedTotalRows} />
      ) : null}
    </div>
  );
}
