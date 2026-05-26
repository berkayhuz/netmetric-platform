"use client";

import {
  type ColumnOrderState,
  flexRender,
  functionalUpdate,
  getCoreRowModel,
  getFacetedRowModel,
  getFacetedUniqueValues,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
  type ColumnDef,
  type ColumnFiltersState,
  type Header,
  type PaginationState,
  type RowSelectionState,
  type SortingState,
  type Updater,
  type VisibilityState,
} from "@tanstack/react-table";
import * as React from "react";

import { cn } from "../../../lib/utils";
import { Checkbox } from "../../primitives/checkbox";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "../table";

import { DataTableColumnHeader } from "./data-table-column-header";
import { DataTableEmptyState } from "./data-table-empty-state";
import { DataTablePagination } from "./data-table-pagination";
import { DataTableSkeleton } from "./data-table-skeleton";
import { DataTableToolbar } from "./data-table-toolbar";

import type {
  DataTableColumnMeta,
  DataTableProps,
  DataTableStateContent,
} from "./data-table-types";

const DEFAULT_PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

function updateState<TValue>(updater: Updater<TValue>, current: TValue): TValue {
  return functionalUpdate(updater, current);
}

function buildControlledStateHandler<TValue>(
  controlledValue: TValue | undefined,
  internalValue: TValue,
  setInternalValue: React.Dispatch<React.SetStateAction<TValue>>,
  onChange?: (value: TValue) => void,
): (updater: Updater<TValue>) => void {
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
    size: 32,
    enableHiding: false,
    enableSorting: false,
    meta: { disableReorder: true },
    header: ({ table }) => (
      <Checkbox
        aria-label="Select all rows on this page"
        checked={table.getIsAllPageRowsSelected()}
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

function includesSelectionColumn<TData>(columns: ColumnDef<TData, unknown>[]): boolean {
  return columns.some((column) => ("id" in column ? column.id === "__select" : false));
}

function toErrorMessage(error: unknown): string | null {
  if (!error) {
    return null;
  }

  if (typeof error === "string") {
    return error;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Unknown error";
}

function toAriaSortValue(sorted: false | "asc" | "desc"): "ascending" | "descending" | "none" {
  if (sorted === "asc") {
    return "ascending";
  }

  if (sorted === "desc") {
    return "descending";
  }

  return "none";
}

function getPaginationDefaults(pagination: PaginationState | undefined): PaginationState {
  return {
    pageIndex: pagination?.pageIndex ?? 0,
    pageSize: pagination?.pageSize ?? 20,
  };
}

function getColumnMeta<TData>(header: Header<TData, unknown>): DataTableColumnMeta | undefined {
  return header.column.columnDef.meta as DataTableColumnMeta | undefined;
}

function renderHeader<TData>(header: Header<TData, unknown>): React.ReactNode {
  if (header.isPlaceholder) {
    return null;
  }

  if (typeof header.column.columnDef.header === "string" && header.column.getCanSort()) {
    return <DataTableColumnHeader column={header.column} title={header.column.columnDef.header} />;
  }

  return flexRender(header.column.columnDef.header, header.getContext());
}

function renderState(
  state: DataTableStateContent | undefined,
  fallbackTitle: string,
  fallbackDescription: string | undefined,
  role: "status" | "alert",
): React.JSX.Element {
  return (
    <DataTableEmptyState
      title={state?.title ?? fallbackTitle}
      description={state?.description ?? fallbackDescription}
      action={state?.action}
      icon={state?.icon}
      role={role}
    />
  );
}

function getResolvedTotalRows(
  mode: "client" | "server",
  totalRows: number | undefined,
  clientRows: number,
): number {
  if (mode === "server") {
    return totalRows ?? clientRows;
  }

  return clientRows;
}

export function DataTable<TData>({
  data,
  columns,
  getRowId,
  mode = "client",
  totalRows,
  loading = false,
  error = null,
  className,
  caption: _caption,
  labels = {},
  emptyState,
  errorState,
  globalFilterPlaceholder,
  enableGlobalFilter = true,
  enableColumnFilters = true,
  enableSorting = true,
  enablePagination = true,
  enableRowSelection = false,
  enableColumnVisibility = true,
  enableColumnReorder = false,
  facetedFilters = [],
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
  skeletonRows = 5,
  sorting,
  columnFilters,
  globalFilter,
  pagination,
  rowSelection,
  columnVisibility,
  columnOrder,
  initialColumnVisibility,
  initialColumnOrder,
  onSortingChange,
  onColumnFiltersChange,
  onGlobalFilterChange,
  onPaginationChange,
  onRowSelectionChange,
  onColumnVisibilityChange,
  onColumnOrderChange,
  toolbarActions,
  renderToolbar,
  renderRowActions,
  renderBulkActions,
  renderEmpty,
  renderLoading,
  renderError,
}: DataTableProps<TData>): React.JSX.Element {
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
    columnVisibility ?? initialColumnVisibility ?? {},
  );
  const [columnOrderState, setColumnOrderState] = React.useState<ColumnOrderState>(
    columnOrder ?? initialColumnOrder ?? [],
  );
  const [draggingColumnId, setDraggingColumnId] = React.useState<string | null>(null);

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
  React.useEffect(() => {
    if (columnOrder) {
      setColumnOrderState(columnOrder);
    }
  }, [columnOrder]);

  React.useEffect(() => {
    if (!isServerMode || totalRows !== undefined || hasWarnedMissingTotalRowsRef.current) {
      return;
    }

    hasWarnedMissingTotalRowsRef.current = true;
    if (typeof console !== "undefined") {
      console.warn(
        "[@netmetric/ui] DataTable mode='server' used without totalRows. Falling back to current data length for pagination display.",
      );
    }
  }, [isServerMode, totalRows]);

  const safeColumns = React.useMemo<ColumnDef<TData, unknown>[]>(() => {
    if (!enableRowSelection || includesSelectionColumn(columns)) {
      return columns;
    }

    return [createSelectionColumn<TData>(), ...columns];
  }, [columns, enableRowSelection]);

  const effectivePagination = pagination ?? paginationState;
  const fallbackTotalRows = totalRows ?? data.length;
  const pageCount = Math.max(
    1,
    Math.ceil(fallbackTotalRows / Math.max(1, effectivePagination.pageSize)),
  );

  const table = useReactTable({
    data,
    columns: safeColumns,
    state: {
      sorting: sorting ?? sortingState,
      columnFilters: columnFilters ?? columnFiltersState,
      globalFilter: globalFilter ?? globalFilterState,
      pagination: effectivePagination,
      rowSelection: rowSelection ?? rowSelectionState,
      columnVisibility: columnVisibility ?? columnVisibilityState,
      columnOrder: columnOrder ?? columnOrderState,
    },
    initialState: {
      columnVisibility: initialColumnVisibility ?? {},
    },
    manualPagination: isServerMode,
    manualSorting: isServerMode,
    manualFiltering: isServerMode,
    enableSorting,
    enableFilters: enableColumnFilters,
    enableGlobalFilter,
    enableRowSelection,
    enableHiding: enableColumnVisibility,
    ...(isServerMode
      ? {
          pageCount,
          rowCount: fallbackTotalRows,
        }
      : {}),
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getFacetedRowModel: getFacetedRowModel(),
    getFacetedUniqueValues: getFacetedUniqueValues(),
    ...(enablePagination && !isServerMode
      ? {
          getPaginationRowModel: getPaginationRowModel(),
        }
      : {}),
    ...(!isServerMode
      ? {
          getSortedRowModel: getSortedRowModel(),
        }
      : {}),
    ...(getRowId ? { getRowId } : {}),
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
    onColumnOrderChange: buildControlledStateHandler(
      columnOrder,
      columnOrderState,
      setColumnOrderState,
      onColumnOrderChange,
    ),
  });

  const reorderColumns = React.useCallback(
    (targetColumnId: string) => {
      if (!draggingColumnId || draggingColumnId === targetColumnId) {
        return;
      }

      table.setColumnOrder((current) => {
        const source =
          current.length > 0 ? [...current] : table.getAllLeafColumns().map((c) => c.id);
        const from = source.indexOf(draggingColumnId);
        const to = source.indexOf(targetColumnId);
        if (from < 0 || to < 0) {
          return source;
        }

        source.splice(from, 1);
        source.splice(to, 0, draggingColumnId);
        return source;
      });
      setDraggingColumnId(null);
    },
    [draggingColumnId, table],
  );

  const canReorderHeader = React.useCallback(
    (header: Header<TData, unknown>): boolean => {
      if (!enableColumnReorder || header.isPlaceholder || header.column.id === "__select") {
        return false;
      }

      const meta = getColumnMeta(header);
      return meta?.disableReorder !== true;
    },
    [enableColumnReorder],
  );

  const rows = table.getRowModel().rows;
  const resolvedError = toErrorMessage(error);
  const selectedRows = table.getSelectedRowModel().rows;
  const resolvedTotalRows = getResolvedTotalRows(
    mode,
    totalRows,
    isServerMode ? data.length : table.getFilteredRowModel().rows.length,
  );
  const visibleColumnCount = Math.max(1, table.getVisibleLeafColumns().length);
  const effectiveColumnCount =
    visibleColumnCount + (typeof renderRowActions === "function" ? 1 : 0);
  const resolvedGlobalFilter = globalFilter ?? globalFilterState;
  const resolvedSearchPlaceholder =
    globalFilterPlaceholder ?? labels.searchPlaceholder ?? "Search...";

  return (
    <div data-slot="data-table" className={cn("flex min-h-0 flex-col", className)}>
      <DataTableToolbar
        table={table}
        globalFilter={resolvedGlobalFilter}
        onGlobalFilterChange={(value) => table.setGlobalFilter(value)}
        enableGlobalFilter={enableGlobalFilter}
        enableColumnVisibility={enableColumnVisibility}
        globalFilterPlaceholder={resolvedSearchPlaceholder}
        selectedRows={selectedRows}
        labels={labels}
        facetedFilters={facetedFilters}
        toolbarActions={toolbarActions}
        renderToolbar={renderToolbar}
        renderBulkActions={renderBulkActions}
      />

      <div className="min-h-full flex-1 overflow-hidden">
        <Table aria-busy={loading || undefined}>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => {
                  const canSort = header.column.getCanSort();
                  const meta = getColumnMeta(header);

                  return (
                    <TableHead
                      key={header.id}
                      aria-sort={canSort ? toAriaSortValue(header.column.getIsSorted()) : undefined}
                      className={cn(
                        meta?.headerClassName,
                        canReorderHeader(header) ? "cursor-grab active:cursor-grabbing" : null,
                      )}
                      draggable={canReorderHeader(header)}
                      onDragStart={() => {
                        if (!canReorderHeader(header)) {
                          return;
                        }
                        setDraggingColumnId(header.column.id);
                      }}
                      onDragEnd={() => {
                        setDraggingColumnId(null);
                      }}
                      onDragOver={(event) => {
                        if (!canReorderHeader(header) || !draggingColumnId) {
                          return;
                        }
                        event.preventDefault();
                      }}
                      onDrop={() => {
                        if (!canReorderHeader(header)) {
                          return;
                        }
                        reorderColumns(header.column.id);
                      }}
                    >
                      {renderHeader(header)}
                    </TableHead>
                  );
                })}
                {typeof renderRowActions === "function" ? (
                  <TableHead className="w-0 whitespace-nowrap text-right">
                    {labels.actions ?? "Actions"}
                  </TableHead>
                ) : null}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {loading ? (
              typeof renderLoading === "function" ? (
                <TableRow>
                  <TableCell colSpan={effectiveColumnCount}>{renderLoading({ table })}</TableCell>
                </TableRow>
              ) : (
                <DataTableSkeleton columnCount={effectiveColumnCount} rowCount={skeletonRows} />
              )
            ) : resolvedError ? (
              <TableRow>
                <TableCell colSpan={effectiveColumnCount}>
                  {typeof renderError === "function"
                    ? renderError({ table, error: resolvedError })
                    : renderState(
                        errorState,
                        labels.errorTitle ?? "Could not load data",
                        errorState?.description ?? labels.errorDescription ?? resolvedError,
                        "alert",
                      )}
                </TableCell>
              </TableRow>
            ) : rows.length === 0 ? (
              <TableRow data-empty="true">
                <TableCell colSpan={effectiveColumnCount} className="p-0">
                  {typeof renderEmpty === "function"
                    ? renderEmpty({ table })
                    : renderState(
                        emptyState,
                        labels.emptyTitle ?? "No results",
                        emptyState?.description ??
                          labels.emptyDescription ??
                          "No rows match your current filters.",
                        "status",
                      )}
                </TableCell>
              </TableRow>
            ) : (
              rows.map((row) => (
                <TableRow key={row.id} data-state={row.getIsSelected() ? "selected" : undefined}>
                  {row.getVisibleCells().map((cell) => {
                    const meta = cell.column.columnDef.meta as DataTableColumnMeta | undefined;

                    return (
                      <TableCell key={cell.id} className={meta?.className}>
                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                      </TableCell>
                    );
                  })}
                  {typeof renderRowActions === "function" ? (
                    <TableCell className="text-right">{renderRowActions(row)}</TableCell>
                  ) : null}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {enablePagination ? (
        <DataTablePagination
          table={table}
          totalRows={resolvedTotalRows}
          pageSizeOptions={pageSizeOptions}
          labels={labels}
        />
      ) : null}
    </div>
  );
}
