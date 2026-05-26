"use client";

import * as React from "react";

import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from "../../navigation/pagination";

import type { Table as TanstackTable } from "@tanstack/react-table";

interface DataGridPaginationProps<TData> {
  table: TanstackTable<TData>;
  totalRows: number;
}

export function DataGridPagination<TData>({
  table,
  totalRows,
}: DataGridPaginationProps<TData>): React.JSX.Element {
  const {
    pagination: { pageIndex, pageSize },
  } = table.getState();

  const safePageSize = pageSize > 0 ? pageSize : 1;
  const pageCount = Math.max(1, Math.ceil(totalRows / safePageSize));
  const canPreviousPage = pageIndex > 0;
  const canNextPage = pageIndex + 1 < pageCount;
  const from = totalRows === 0 ? 0 : pageIndex * safePageSize + 1;
  const to = Math.min(totalRows, (pageIndex + 1) * safePageSize);

  return (
    <div className="flex flex-col gap-3 border-t pt-3 md:flex-row md:items-center md:justify-between">
      <p className="text-muted-foreground text-sm">
        Showing {from}-{to} of {totalRows}
      </p>
      <div className="flex items-center gap-3">
        <span className="text-sm">
          Page {Math.min(pageIndex + 1, pageCount)} / {pageCount}
        </span>
        <Pagination className="mx-0 w-auto justify-start">
          <PaginationContent>
            <PaginationItem>
              <PaginationPrevious
                href="#"
                aria-disabled={!canPreviousPage}
                className={!canPreviousPage ? "pointer-events-none opacity-50" : undefined}
                onClick={(event) => {
                  event.preventDefault();
                  if (canPreviousPage) {
                    table.setPageIndex(pageIndex - 1);
                  }
                }}
              />
            </PaginationItem>
            <PaginationItem>
              <PaginationNext
                href="#"
                aria-disabled={!canNextPage}
                className={!canNextPage ? "pointer-events-none opacity-50" : undefined}
                onClick={(event) => {
                  event.preventDefault();
                  if (canNextPage) {
                    table.setPageIndex(pageIndex + 1);
                  }
                }}
              />
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      </div>
    </div>
  );
}
