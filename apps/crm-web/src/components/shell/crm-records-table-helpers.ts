import type { CrmRecordsTableColumn, CrmRecordsTableRow } from "./crm-records-table";

type SortDirection = "asc" | "desc";

export function compareCrmRecordValues(
  left: string | number | undefined,
  right: string | number | undefined,
  direction: SortDirection,
): number {
  const leftText = left == null ? "" : String(left);
  const rightText = right == null ? "" : String(right);
  const numericLeft = Number(leftText);
  const numericRight = Number(rightText);
  const bothNumeric = Number.isFinite(numericLeft) && Number.isFinite(numericRight);
  const result = bothNumeric
    ? numericLeft - numericRight
    : leftText.localeCompare(rightText, undefined, { sensitivity: "base", numeric: true });

  return direction === "asc" ? result : -result;
}

export function formatCrmRecordCellDisplayValue(value: string): string {
  const dateTimePattern = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+\-]\d{2}:\d{2})$/;
  const dateOnlyPattern = /^\d{4}-\d{2}-\d{2}$/;

  if (dateTimePattern.test(value)) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat(undefined, {
      dateStyle: "medium",
      timeStyle: "short",
    }).format(date);
  }

  if (dateOnlyPattern.test(value)) {
    const date = new Date(`${value}T00:00:00`);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(date);
  }

  return value;
}

export function getCrmRecordCellValue(row: CrmRecordsTableRow, key: string): string {
  const value = row.cells[key];
  if (!value || value.trim().length === 0) {
    return "-";
  }

  return formatCrmRecordCellDisplayValue(value);
}

export function getCrmRecordSearchHaystack(row: CrmRecordsTableRow): string {
  return (
    row.searchText ??
    [...Object.values(row.cells), ...Object.values(row.descriptions ?? {}).filter(Boolean)].join(
      " ",
    )
  );
}

export function resolveCrmRecordFilterValue(
  row: CrmRecordsTableRow,
  key: string,
): string | undefined {
  return row.filterValues?.[key] ?? row.cells[key];
}

export function getCrmRecordBadgeClassName(value: string): string | undefined {
  const normalized = value.toLocaleLowerCase("en-US");

  if (
    ["active", "aktif", "yes", "true", "won", "open", "current", "enabled"].includes(normalized)
  ) {
    return "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300";
  }

  if (["inactive", "pasif", "no", "false", "lost", "closed", "disabled"].includes(normalized)) {
    return "bg-muted text-muted-foreground";
  }

  if (["high", "urgent", "breached", "failed", "overdue"].includes(normalized)) {
    return "bg-rose-500/15 text-rose-700 dark:text-rose-300";
  }

  if (["medium", "pending", "waiting"].includes(normalized)) {
    return "bg-amber-500/15 text-amber-700 dark:text-amber-300";
  }

  return undefined;
}

export function shouldUseCrmRecordBadgeColumn(column: CrmRecordsTableColumn): boolean {
  if (column.badge !== undefined) {
    return column.badge;
  }

  return /status|state|active|priority|stage|default|enabled|linked|sla/i.test(column.key);
}

export function createDefaultCrmRecordFilterValues(
  filters: Array<{ key: string }>,
): Record<string, string> {
  return Object.fromEntries(filters.map((filter) => [filter.key, "all"]));
}
