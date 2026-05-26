"use client";

import { ChevronsLeft, ChevronsRight, ChevronLeft, ChevronRight } from "lucide-react";
import * as React from "react";

import { Button } from "../../primitives/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../primitives/select";

import type { DataTableLabels } from "./data-table-types";
import type { Table as TanstackTable } from "@tanstack/react-table";

interface DataTablePaginationProps<TData> {
  table: TanstackTable<TData>;
  totalRows: number;
  pageSizeOptions: number[];
  labels: DataTableLabels;
}

export function DataTablePagination<TData>({
  table,
  totalRows,
  pageSizeOptions,
  labels,
}: DataTablePaginationProps<TData>): React.JSX.Element | null {
  const {
    pagination: { pageIndex, pageSize },
  } = table.getState();
  const safePageSize = pageSize > 0 ? pageSize : 1;
  const pageCount = Math.max(1, table.getPageCount());
  const from = totalRows === 0 ? 0 : pageIndex * safePageSize + 1;
  const to = Math.min(totalRows, (pageIndex + 1) * safePageSize);

  if (pageCount <= 1 && totalRows <= safePageSize) {
    return null;
  }

  return (
    <div className="flex flex-col gap-3 border-t pt-3 md:flex-row md:items-center md:justify-between">
      <p className="text-sm text-muted-foreground">
        {from}-{to} / {totalRows}
      </p>
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">
            {labels.rowsPerPage ?? "Rows per page"}
          </span>
          <Select
            value={String(pageSize)}
            onValueChange={(value) => {
              table.setPageSize(Number(value));
              table.setPageIndex(0);
            }}
          >
            <SelectTrigger size="sm" className="w-20">
              <SelectValue />
            </SelectTrigger>
            <SelectContent align="end">
              {pageSizeOptions.map((option) => (
                <SelectItem key={option} value={String(option)}>
                  {option}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="flex items-center justify-between gap-2 sm:justify-start">
          <span className="min-w-24 text-sm">
            {labels.page ?? "Page"} {Math.min(pageIndex + 1, pageCount)} / {pageCount}
          </span>
          <div className="flex items-center gap-1">
            <Button
              type="button"
              variant="outline"
              size="icon-sm"
              aria-label={labels.firstPage ?? "Go to first page"}
              disabled={!table.getCanPreviousPage()}
              onClick={() => table.setPageIndex(0)}
            >
              <ChevronsLeft aria-hidden="true" />
            </Button>
            <Button
              type="button"
              variant="outline"
              size="icon-sm"
              aria-label={labels.previousPage ?? "Go to previous page"}
              disabled={!table.getCanPreviousPage()}
              onClick={() => table.previousPage()}
            >
              <ChevronLeft aria-hidden="true" />
            </Button>
            <Button
              type="button"
              variant="outline"
              size="icon-sm"
              aria-label={labels.nextPage ?? "Go to next page"}
              disabled={!table.getCanNextPage()}
              onClick={() => table.nextPage()}
            >
              <ChevronRight aria-hidden="true" />
            </Button>
            <Button
              type="button"
              variant="outline"
              size="icon-sm"
              aria-label={labels.lastPage ?? "Go to last page"}
              disabled={!table.getCanNextPage()}
              onClick={() => table.setPageIndex(pageCount - 1)}
            >
              <ChevronsRight aria-hidden="true" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
