"use client";

import { flexRender, type Row, type Table as TanstackTable } from "@tanstack/react-table";
import { ArrowDown, ArrowUp, ArrowUpDown } from "lucide-react";
import * as React from "react";

import { Skeleton } from "../../layout/skeleton";
import { Button } from "../../primitives/button";
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from "../empty";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "../table";

interface DataGridTableProps<TData> {
  table: TanstackTable<TData>;
  loading: boolean;
  error: string | null;
  renderRowActions?: (row: Row<TData>) => React.ReactNode;
  renderEmpty?: (context: { table: TanstackTable<TData> }) => React.ReactNode;
  renderLoading?: (context: { table: TanstackTable<TData> }) => React.ReactNode;
  renderError?: (context: { table: TanstackTable<TData>; error: string }) => React.ReactNode;
}

function SortIndicator({ sorted }: { sorted: false | "asc" | "desc" }): React.JSX.Element {
  if (sorted === "asc") {
    return <ArrowUp aria-hidden="true" className="h-4 w-4" />;
  }
  if (sorted === "desc") {
    return <ArrowDown aria-hidden="true" className="h-4 w-4" />;
  }
  return <ArrowUpDown aria-hidden="true" className="h-4 w-4 text-muted-foreground" />;
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

export function DataGridTable<TData>({
  table,
  loading,
  error,
  renderRowActions,
  renderEmpty,
  renderLoading,
  renderError,
}: DataGridTableProps<TData>): React.JSX.Element {
  const headerGroups = table.getHeaderGroups();
  const rows = table.getRowModel().rows;
  const visibleColumnCount = Math.max(1, table.getVisibleLeafColumns().length);
  const effectiveColumnCount =
    visibleColumnCount + (typeof renderRowActions === "function" ? 1 : 0);

  return (
    <div className="space-y-3">
      <Table aria-busy={loading || undefined}>
        <TableHeader>
          {headerGroups.map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => {
                if (header.isPlaceholder) {
                  return <TableHead key={header.id} />;
                }

                const canSort = header.column.getCanSort();
                const sorted = header.column.getIsSorted();

                return (
                  <TableHead
                    key={header.id}
                    aria-sort={canSort ? toAriaSortValue(sorted) : undefined}
                  >
                    {canSort ? (
                      <Button
                        className="-ml-2 h-8 px-2"
                        type="button"
                        variant="ghost"
                        onClick={header.column.getToggleSortingHandler()}
                      >
                        <span>
                          {flexRender(header.column.columnDef.header, header.getContext())}
                        </span>
                        <SortIndicator sorted={sorted} />
                      </Button>
                    ) : (
                      flexRender(header.column.columnDef.header, header.getContext())
                    )}
                  </TableHead>
                );
              })}
              {typeof renderRowActions === "function" ? <TableHead>Actions</TableHead> : null}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {loading ? (
            <TableRow>
              <TableCell colSpan={effectiveColumnCount}>
                {typeof renderLoading === "function" ? (
                  renderLoading({ table })
                ) : (
                  <div className="space-y-2 p-2" role="status" aria-live="polite">
                    <Skeleton className="h-4 w-full" />
                    <Skeleton className="h-4 w-full" />
                    <Skeleton className="h-4 w-4/5" />
                  </div>
                )}
              </TableCell>
            </TableRow>
          ) : error ? (
            <TableRow>
              <TableCell colSpan={effectiveColumnCount}>
                {typeof renderError === "function" ? (
                  renderError({ table, error })
                ) : (
                  <Empty className="border-none p-4" role="alert">
                    <EmptyHeader>
                      <EmptyTitle>Could not load data</EmptyTitle>
                      <EmptyDescription>{error}</EmptyDescription>
                    </EmptyHeader>
                  </Empty>
                )}
              </TableCell>
            </TableRow>
          ) : rows.length === 0 ? (
            <TableRow>
              <TableCell colSpan={effectiveColumnCount}>
                {typeof renderEmpty === "function" ? (
                  renderEmpty({ table })
                ) : (
                  <Empty className="border-none p-4" role="status" aria-live="polite">
                    <EmptyHeader>
                      <EmptyTitle>No results</EmptyTitle>
                      <EmptyDescription>No rows match your current filters.</EmptyDescription>
                    </EmptyHeader>
                  </Empty>
                )}
              </TableCell>
            </TableRow>
          ) : (
            rows.map((row) => (
              <TableRow key={row.id} data-state={row.getIsSelected() ? "selected" : undefined}>
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </TableCell>
                ))}
                {typeof renderRowActions === "function" ? (
                  <TableCell>{renderRowActions(row)}</TableCell>
                ) : null}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
