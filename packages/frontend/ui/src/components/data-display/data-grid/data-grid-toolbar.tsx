"use client";

import * as React from "react";

import { Button } from "../../primitives/button";
import { Input } from "../../primitives/input";

import type { Row, Table as TanstackTable } from "@tanstack/react-table";

interface DataGridToolbarProps<TData> {
  table: TanstackTable<TData>;
  globalFilter: string;
  onGlobalFilterChange: (value: string) => void;
  globalFilterPlaceholder: string;
  enableGlobalFilter: boolean;
  selectedRows: Row<TData>[];
  renderToolbar?: (context: {
    table: TanstackTable<TData>;
    globalFilter: string;
    selectedCount: number;
  }) => React.ReactNode;
  renderBulkActions?: (rows: Row<TData>[]) => React.ReactNode;
}

export function DataGridToolbar<TData>({
  table,
  globalFilter,
  onGlobalFilterChange,
  globalFilterPlaceholder,
  enableGlobalFilter,
  selectedRows,
  renderToolbar,
  renderBulkActions,
}: DataGridToolbarProps<TData>): React.JSX.Element | null {
  const selectedCount = selectedRows.length;
  const hasToolbar = enableGlobalFilter || selectedCount > 0 || typeof renderToolbar === "function";
  if (!hasToolbar) {
    return null;
  }

  return (
    <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <div className="flex items-center gap-2">
        {enableGlobalFilter ? (
          <Input
            aria-label="Search rows"
            className="w-full md:w-72"
            placeholder={globalFilterPlaceholder}
            value={globalFilter}
            onChange={(event) => onGlobalFilterChange(event.target.value)}
          />
        ) : null}
        {typeof renderToolbar === "function"
          ? renderToolbar({ table, globalFilter, selectedCount })
          : null}
      </div>
      {selectedCount > 0 ? (
        <div className="flex items-center gap-2">
          <span className="text-muted-foreground text-sm">{selectedCount} selected</span>
          {typeof renderBulkActions === "function" ? (
            renderBulkActions(selectedRows)
          ) : (
            <Button size="sm" variant="outline" type="button">
              Bulk actions
            </Button>
          )}
        </div>
      ) : null}
    </div>
  );
}
