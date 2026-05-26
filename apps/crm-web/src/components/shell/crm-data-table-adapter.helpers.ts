import type { CrmRecordsTableColumn, CrmRecordsTableRow } from "./crm-records-table";
import {
  compareCrmRecordValues,
  getCrmRecordBadgeClassName,
  getCrmRecordCellValue,
  getCrmRecordSearchHaystack,
  resolveCrmRecordFilterValue,
  shouldUseCrmRecordBadgeColumn,
} from "./crm-records-table-helpers";
import type {
  CrmDataTableColumnMapping,
  CrmDataTableFilterMapping,
  CrmDataTableRowData,
} from "./crm-data-table-adapter.types";

export function createCrmDataTableRowData(rows: CrmRecordsTableRow[]): CrmDataTableRowData[] {
  return rows;
}

export function mapCrmRowsForDataTable(rows: CrmRecordsTableRow[]): CrmDataTableRowData[] {
  return createCrmDataTableRowData(rows);
}

export function createCrmDataTableColumnMappings(
  columns: CrmRecordsTableColumn[],
): CrmDataTableColumnMapping[] {
  return columns.map((column) => ({
    source: column,
    target: {
      key: column.key,
      header: column.header,
      sortable: column.sortable ?? true,
      badge: shouldUseCrmRecordBadgeColumn(column),
      className: column.className,
    },
  }));
}

export function createCrmColumnDescriptors(
  columns: CrmRecordsTableColumn[],
): CrmDataTableColumnMapping[] {
  return createCrmDataTableColumnMappings(columns);
}

export function createCrmGlobalFilterFn() {
  return (row: CrmDataTableRowData, rawSearch: string): boolean => {
    const term = rawSearch.trim().toLocaleLowerCase();
    if (term.length === 0) {
      return true;
    }

    return getCrmRecordSearchHaystack(row).toLocaleLowerCase().includes(term);
  };
}

export function createCrmDataTableSearchConfig() {
  return {
    globalFilterFn: createCrmGlobalFilterFn(),
  } as const;
}

export function createCrmFilterPredicate(filterKey: string, selectedValue: string) {
  return (row: CrmDataTableRowData): boolean => {
    if (selectedValue === "all") {
      return true;
    }

    return resolveCrmRecordFilterValue(row, filterKey) === selectedValue;
  };
}

export function createCrmSortingFn(columnKey: string) {
  return (
    left: CrmDataTableRowData,
    right: CrmDataTableRowData,
    direction: "asc" | "desc",
  ): number => {
    return compareCrmRecordValues(
      left.sortValues?.[columnKey] ?? left.cells[columnKey],
      right.sortValues?.[columnKey] ?? right.cells[columnKey],
      direction,
    );
  };
}

export function createCrmDataTableSortConfig(columnKey: string) {
  return {
    columnKey,
    sortingFn: createCrmSortingFn(columnKey),
  } as const;
}

export function sortCrmRows(
  rows: CrmDataTableRowData[],
  sort: Readonly<{ key: string; direction: "asc" | "desc" }>,
): CrmDataTableRowData[] {
  return [...rows].sort((left, right) =>
    createCrmDataTableSortConfig(sort.key).sortingFn(left, right, sort.direction),
  );
}

export function createCrmCellRenderDescriptor(
  column: CrmRecordsTableColumn,
  columnIndex: number,
): Readonly<{
  key: string;
  header: string;
  isFirstColumn: boolean;
  useBadge: boolean;
  className?: string;
}> {
  const descriptor: {
    key: string;
    header: string;
    isFirstColumn: boolean;
    useBadge: boolean;
    className?: string;
  } = {
    key: column.key,
    header: column.header,
    isFirstColumn: columnIndex === 0,
    useBadge: shouldUseCrmRecordBadgeColumn(column),
  };

  if (column.className !== undefined) {
    descriptor.className = column.className;
  }

  return descriptor;
}

export function resolveCrmCellRenderValue(
  row: CrmDataTableRowData,
  columnKey: string,
): Readonly<{
  value: string;
  description?: string;
  badgeClassName?: string;
}> {
  const value = getCrmRecordCellValue(row, columnKey);
  const resolved: {
    value: string;
    description?: string;
    badgeClassName?: string;
  } = {
    value,
  };

  const description = row.descriptions?.[columnKey];
  if (description !== undefined) {
    resolved.description = description;
  }

  const badgeClassName = getCrmRecordBadgeClassName(value);
  if (badgeClassName !== undefined) {
    resolved.badgeClassName = badgeClassName;
  }

  return resolved;
}

export function createCrmDataTableFilterMappings(
  filters: Array<{
    key: string;
    label: string;
    allLabel: string;
    options: Array<{ value: string; label: string }>;
  }>,
): CrmDataTableFilterMapping[] {
  return filters.map((filter) => ({
    key: filter.key,
    label: filter.label,
    allLabel: filter.allLabel,
    options: filter.options,
  }));
}

export function mapCrmFiltersForDataTable(
  filters: Array<{
    key: string;
    label: string;
    allLabel: string;
    options: Array<{ value: string; label: string }>;
  }>,
): CrmDataTableFilterMapping[] {
  return createCrmDataTableFilterMappings(filters);
}

export function getCrmSelectedVisibleCount(
  visibleRows: CrmDataTableRowData[],
  selectedIds: ReadonlySet<string>,
): number {
  return visibleRows.reduce((count, row) => (selectedIds.has(row.id) ? count + 1 : count), 0);
}

export function areAllVisibleCrmRowsSelected(
  visibleRows: CrmDataTableRowData[],
  selectedIds: ReadonlySet<string>,
): boolean {
  return visibleRows.length > 0 && visibleRows.every((row) => selectedIds.has(row.id));
}
