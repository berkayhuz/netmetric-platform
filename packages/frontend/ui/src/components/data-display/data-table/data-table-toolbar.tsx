"use client";

import { Search, X } from "lucide-react";
import * as React from "react";

import { Button } from "../../primitives/button";
import { Input } from "../../primitives/input";

import { DataTableFacetedFilter } from "./data-table-faceted-filter";
import { DataTableViewOptions } from "./data-table-view-options";

import type {
  DataTableFacetedFilter as DataTableFacetedFilterConfig,
  DataTableLabels,
  DataTableToolbarContext,
} from "./data-table-types";
import type { Row, Table as TanstackTable } from "@tanstack/react-table";

interface DataTableToolbarProps<TData> {
  table: TanstackTable<TData>;
  globalFilter: string;
  onGlobalFilterChange: (value: string) => void;
  enableGlobalFilter: boolean;
  enableColumnVisibility: boolean;
  globalFilterPlaceholder: string;
  selectedRows: Row<TData>[];
  labels: DataTableLabels;
  facetedFilters: DataTableFacetedFilterConfig[];
  toolbarActions?:
    | React.ReactNode
    | ((context: DataTableToolbarContext<TData>) => React.ReactNode)
    | undefined;
  renderToolbar?: ((context: DataTableToolbarContext<TData>) => React.ReactNode) | undefined;
  renderBulkActions?: ((rows: Row<TData>[]) => React.ReactNode) | undefined;
}

export function DataTableToolbar<TData>({
  table,
  globalFilter,
  onGlobalFilterChange,
  enableGlobalFilter,
  enableColumnVisibility,
  globalFilterPlaceholder,
  selectedRows,
  labels,
  facetedFilters,
  toolbarActions,
  renderToolbar,
  renderBulkActions: _renderBulkActions,
}: DataTableToolbarProps<TData>): React.JSX.Element | null {
  const selectedCount = selectedRows.length;
  const context: DataTableToolbarContext<TData> = {
    table,
    globalFilter,
    selectedCount,
    selectedRows,
  };
  const hasFilters = table.getState().columnFilters.length > 0 || globalFilter.length > 0;
  const hasToolbar =
    enableGlobalFilter ||
    facetedFilters.length > 0 ||
    selectedCount > 0 ||
    enableColumnVisibility ||
    Boolean(toolbarActions) ||
    typeof renderToolbar === "function";

  if (!hasToolbar) {
    return null;
  }

  const actionContent =
    typeof toolbarActions === "function" ? toolbarActions(context) : toolbarActions;

  return (
    <div className="flex flex-col">
      {/* ÜST SATIR: inputlar, filtreler, butonlar */}
      <div className="flex min-w-0 flex-wrap items-center gap-2 p-2">
        {enableGlobalFilter ? (
          <div className="relative w-full sm:w-72">
            <Search
              aria-hidden="true"
              className="pointer-events-none absolute left-2.5 top-1/2 size-3.5 -translate-y-1/2 text-muted-foreground"
            />
            <Input
              aria-label={labels.search ?? "Search rows"}
              className="h-8 pl-8"
              placeholder={globalFilterPlaceholder}
              value={globalFilter}
              onChange={(event) => onGlobalFilterChange(event.target.value)}
            />
          </div>
        ) : null}

        {facetedFilters.map((filter) => (
          <DataTableFacetedFilter
            key={filter.columnId}
            table={table}
            columnId={filter.columnId}
            title={filter.title}
            options={filter.options}
            multiple={filter.multiple}
            emptyLabel={filter.emptyLabel}
            clearLabel={filter.clearLabel ?? labels.reset}
          />
        ))}

        {hasFilters ? (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="h-8 w-fit"
            onClick={() => {
              table.resetColumnFilters();
              table.setGlobalFilter("");
            }}
          >
            <X aria-hidden="true" />
            {labels.reset ?? "Reset"}
          </Button>
        ) : null}

        {enableColumnVisibility ? (
          <DataTableViewOptions
            table={table}
            label={labels.viewColumns ?? "View columns"}
            title={labels.columns ?? "Columns"}
          />
        ) : null}

        {actionContent}
      </div>

      {/* ALT SATIR: shown / selected / bulk actions */}
      <div className="flex min-h-8 flex-wrap items-center gap-2 text-sm text-muted-foreground">
        {typeof renderToolbar === "function" ? renderToolbar(context) : null}
      </div>
    </div>
  );
}
