"use client";

import * as React from "react";

import { cn } from "../../lib/utils";

import { DataTable } from "./data-table";

import type {
  DataTableProps,
  DataTableStateContent,
  DataTableToolbarContext,
} from "./data-table/data-table-types";

export type EntityDataTableDensity = "compact" | "comfortable";

export type EntityDataTableProps<TData> = Omit<
  DataTableProps<TData>,
  "enableGlobalFilter" | "enableColumnFilters" | "emptyState" | "errorState"
> & {
  enableSearch?: boolean;
  enableFilters?: boolean;
  showEmptyState?: boolean;
  showErrorState?: boolean;
  density?: EntityDataTableDensity;
  emptyState?: DataTableStateContent | null;
  errorState?: DataTableStateContent | null;
};

const compactDensityClassName =
  "[&_[data-slot='data-table']_input]:h-8 [&_[data-slot='data-table']_button]:h-8";

export function EntityDataTable<TData>({
  data,
  columns,
  enableSearch,
  enableFilters,
  enablePagination = true,
  enableColumnVisibility = true,
  enableSorting = true,
  enableRowSelection = false,
  showEmptyState = true,
  showErrorState = true,
  emptyState,
  errorState,
  density = "compact",
  className,
  ...props
}: EntityDataTableProps<TData>): React.JSX.Element {
  const resolvedEnableSearch = enableSearch ?? true;
  const resolvedEnableFilters = enableFilters ?? true;

  return (
    <div className={cn(density === "compact" ? compactDensityClassName : null, className)}>
      <DataTable
        data={data}
        columns={columns}
        {...props}
        enableGlobalFilter={resolvedEnableSearch}
        enableColumnFilters={resolvedEnableFilters}
        enablePagination={enablePagination}
        enableColumnVisibility={enableColumnVisibility}
        enableSorting={enableSorting}
        enableRowSelection={enableRowSelection}
        {...(showEmptyState ? {} : { renderEmpty: () => null })}
        {...(showErrorState ? {} : { renderError: () => null })}
        {...(showEmptyState && emptyState !== null ? { emptyState } : {})}
        {...(showErrorState && errorState !== null ? { errorState } : {})}
      />
    </div>
  );
}

export type EntityDataTableToolbarContext<TData> = DataTableToolbarContext<TData>;
