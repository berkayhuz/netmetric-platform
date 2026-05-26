"use client";

import { type ReactNode, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { Badge } from "@netmetric/ui";
import { Checkbox, type DataTableColumnDef } from "@netmetric/ui/client";
import { Filter } from "lucide-react";
import type { DataTableStateContent } from "@netmetric/ui/client";

import type {
  CrmRecordsTableColumn,
  CrmRecordsTableFilter,
  CrmRecordsTableLabels,
  CrmRecordsTableRow,
} from "./crm-records-table";
import {
  createCrmColumnDescriptors,
  mapCrmRowsForDataTable,
  resolveCrmCellRenderValue,
  areAllVisibleCrmRowsSelected,
} from "./crm-data-table-adapter.helpers";
import { CrmEntityDataTable } from "./crm-entity-data-table";
import { compareCrmRecordValues } from "./crm-records-table-helpers";

type CrmDataTableAdapterProps = Readonly<{
  caption: string;
  columns: CrmRecordsTableColumn[];
  rows: CrmRecordsTableRow[];
  filters?: CrmRecordsTableFilter[];
  initialSearch?: string;
  loading?: boolean;
  error?: unknown;
  labels?: CrmRecordsTableLabels;
  minWidthClassName?: string;
  toolbarContent?: ReactNode;
  filterBarLeadingContent?: ReactNode;
  infoContent?: ReactNode;
  onSelectionChange?: (selectedIds: string[]) => void;
  emptyState?: DataTableStateContent;
  selectionActions?: ReactNode | ((context: { selectedCount: number }) => ReactNode);
  enableSelection?: boolean;
  rowActions?: (row: CrmRecordsTableRow) => ReactNode;
}>;

export function CrmDataTableAdapter({
  caption,
  columns,
  rows,
  filters = [],
  initialSearch = "",
  loading = false,
  error = null,
  labels = {},
  minWidthClassName = "min-w-[860px]",
  toolbarContent,
  filterBarLeadingContent,
  infoContent,
  onSelectionChange,
  emptyState,
  selectionActions,
  enableSelection,
  rowActions,
}: CrmDataTableAdapterProps) {
  const activeFilters = filters.slice(0, 2);
  const [globalFilter, setGlobalFilter] = useState(initialSearch);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(() => new Set());

  const sourceRows = useMemo(() => mapCrmRowsForDataTable(rows), [rows]);

  const resolvedEnableSelection =
    enableSelection ?? (Boolean(selectionActions) || Boolean(onSelectionChange));

  const columnDescriptors = useMemo(() => createCrmColumnDescriptors(columns), [columns]);
  const allVisibleSelected = useMemo(
    () => areAllVisibleCrmRowsSelected(sourceRows, selectedIds),
    [selectedIds, sourceRows],
  );
  const dataTableColumns = useMemo<DataTableColumnDef<CrmRecordsTableRow>[]>(() => {
    const selectionColumn: DataTableColumnDef<CrmRecordsTableRow> = {
      id: "__select",
      meta: { disableReorder: true },
      header: () => (
        <Checkbox
          aria-label={labels.selectAll ?? `Select all ${caption}`}
          checked={allVisibleSelected}
          onCheckedChange={(value) => {
            const checked = Boolean(value);
            setSelectedIds((current) => {
              const next = new Set(current);
              for (const row of sourceRows) {
                if (checked) next.add(row.id);
                else next.delete(row.id);
              }
              return next;
            });
          }}
        />
      ),
      cell: ({ row }) => (
        <Checkbox
          aria-label={
            labels.selectRow?.(getRowLabel(row.original, columns)) ??
            `Select ${getRowLabel(row.original, columns)}`
          }
          checked={selectedIds.has(row.original.id)}
          onCheckedChange={(value) => {
            const checked = Boolean(value);
            setSelectedIds((current) => {
              const next = new Set(current);
              if (checked) next.add(row.original.id);
              else next.delete(row.original.id);
              return next;
            });
          }}
        />
      ),
    };

    const mappedColumns: DataTableColumnDef<CrmRecordsTableRow>[] = columnDescriptors.map(
      (descriptor, columnIndex) => ({
        id: descriptor.source.key,
        accessorFn: (row) => row.cells[descriptor.source.key],
        header: descriptor.source.header,
        enableSorting: descriptor.source.sortable ?? true,
        sortingFn: (left, right) =>
          compareCrmRecordValues(
            left.original.sortValues?.[descriptor.source.key] ??
              left.original.cells[descriptor.source.key],
            right.original.sortValues?.[descriptor.source.key] ??
              right.original.cells[descriptor.source.key],
            "asc",
          ),
        filterFn: (row, columnId, value) => {
          if (!Array.isArray(value) || value.length === 0) {
            return true;
          }
          const cellValue = resolveCrmCellRenderValue(row.original, columnId).value;
          return value.includes(cellValue);
        },
        cell: ({ row }) => {
          const resolved = resolveCrmCellRenderValue(row.original, descriptor.source.key);
          const content =
            columnIndex === 0 && row.original.href ? (
              <Link
                href={row.original.href}
                prefetch={false}
                className="font-medium hover:underline"
              >
                {resolved.value}
              </Link>
            ) : descriptor.target &&
              typeof descriptor.target === "object" &&
              "badge" in descriptor.target &&
              descriptor.target.badge ? (
              <Badge variant="secondary" className={resolved.badgeClassName}>
                {resolved.value}
              </Badge>
            ) : (
              resolved.value
            );

          if (!resolved.description) {
            return content;
          }

          return (
            <div className="space-y-1 whitespace-normal">
              <div>{content}</div>
              <p className="line-clamp-2 text-xs font-normal text-muted-foreground">
                {resolved.description}
              </p>
            </div>
          );
        },
      }),
    );

    if (!resolvedEnableSelection || sourceRows.length === 0) {
      return mappedColumns;
    }

    return [selectionColumn, ...mappedColumns];
  }, [
    allVisibleSelected,
    caption,
    columnDescriptors,
    columns,
    labels,
    resolvedEnableSelection,
    selectedIds,
    sourceRows,
  ]);

  const rowSelectionState = useMemo(
    () => Object.fromEntries([...selectedIds].map((id) => [id, true] as const)),
    [selectedIds],
  );
  useEffect(() => {
    onSelectionChange?.(Array.from(selectedIds));
  }, [onSelectionChange, selectedIds]);
  useEffect(() => {
    const visibleIds = new Set(sourceRows.map((row) => row.id));
    setSelectedIds((current) => {
      const next = new Set([...current].filter((id) => visibleIds.has(id)));
      return next.size === current.size ? current : next;
    });
  }, [sourceRows]);

  const clearLabel = labels.clear ?? "Clear";
  const selectedLabel = labels.selected ?? "selected";
  const selectedRowsLabel = `{count} ${selectedLabel}`;
  const dataTableLabels = useMemo(() => {
    const resolved: {
      reset: string;
      selectedRows: string;
      actions?: string;
      emptyTitle?: string;
      emptyDescription?: string;
    } = {
      reset: clearLabel,
      selectedRows: selectedRowsLabel,
    };

    if (labels.selectAll !== undefined) {
      resolved.actions = labels.selectAll;
    }
    if (labels.emptyTitle !== undefined) {
      resolved.emptyTitle = labels.emptyTitle;
    }
    if (labels.emptyDescription !== undefined) {
      resolved.emptyDescription = labels.emptyDescription;
    }

    return resolved;
  }, [clearLabel, labels, selectedRowsLabel]);
  const facetedFilters = useMemo(
    () =>
      activeFilters.map((filter) => ({
        columnId: filter.key,
        title: filter.label,
        options: filter.options,
        multiple: true,
        clearLabel: clearLabel,
      })),
    [activeFilters, clearLabel],
  );
  const shownLabel = labels.shown ?? "shown";

  return (
    <CrmEntityDataTable
      data={sourceRows}
      columns={dataTableColumns}
      caption={caption}
      enableSearch
      enableFilters
      facetedFilters={facetedFilters}
      enableColumnVisibility
      enablePagination={false}
      enableRowSelection={resolvedEnableSelection && sourceRows.length > 0}
      enableSorting
      density="compact"
      showEmptyState
      showErrorState
      loading={loading}
      error={error}
      globalFilter={globalFilter}
      onGlobalFilterChange={setGlobalFilter}
      getRowId={(row) => row.id}
      rowSelection={rowSelectionState}
      onRowSelectionChange={(resolved) => {
        const nextSelectedIds = new Set(
          Object.entries(resolved)
            .filter(([, selected]) => selected)
            .map(([id]) => id),
        );
        setSelectedIds(nextSelectedIds);
      }}
      toolbarActions={toolbarContent}
      renderToolbar={({ table }) => (
        <>
          <div className="flex items-center gap-2">
            {filterBarLeadingContent ? (
              <span className="inline-flex items-center">{filterBarLeadingContent}</span>
            ) : null}
            <span className="inline-flex items-center gap-1.5">
              <Filter aria-hidden="true" className="size-3.5" />
              {table.getFilteredRowModel().rows.length} {shownLabel}
            </span>
          </div>
          {resolvedEnableSelection ? (
            <span>
              {table.getSelectedRowModel().rows.length} {selectedLabel}
            </span>
          ) : null}
        </>
      )}
      labels={dataTableLabels}
      minWidthClassName={minWidthClassName}
      infoContent={infoContent}
      selectionActions={
        resolvedEnableSelection && typeof selectionActions === "function"
          ? ({ selectedCount }) => selectionActions({ selectedCount })
          : resolvedEnableSelection
            ? selectionActions
            : undefined
      }
      {...(rowActions ? { rowActions } : {})}
      {...(emptyState ? { emptyState } : {})}
    />
  );
}

function getRowLabel(row: CrmRecordsTableRow, columns: CrmRecordsTableColumn[]): string {
  const firstColumn = columns[0];
  if (!firstColumn) {
    return row.id;
  }

  return resolveCrmCellRenderValue(row, firstColumn.key).value;
}
