import type { ReactNode } from "react";

import type {
  CrmRecordsTableColumn,
  CrmRecordsTableFilter,
  CrmRecordsTableLabels,
  CrmRecordsTableRow,
} from "./crm-records-table";

/**
 * Type-only scaffold for future CrmRecordsTable -> DataTable adapter migration.
 * This file intentionally contains no runtime wiring.
 */
export type CrmDataTableAdapterProps = Readonly<{
  caption: string;
  columns: CrmRecordsTableColumn[];
  rows: CrmRecordsTableRow[];
  filters?: CrmRecordsTableFilter[];
  initialSearch?: string;
  labels?: CrmRecordsTableLabels;
  minWidthClassName?: string;
  toolbarContent?: ReactNode;
}>;

/**
 * Internal row shape candidate for DataTable mapping.
 * Keeps parity with existing CrmRecordsTableRow contract.
 */
export type CrmDataTableRowData = Readonly<{
  id: string;
  href?: string;
  cells: Record<string, string>;
  descriptions?: Record<string, string | undefined>;
  searchText?: string;
  sortValues?: Record<string, string | number | undefined>;
  filterValues?: Record<string, string | undefined>;
}>;

/**
 * Column mapping descriptor between CrmRecordsTableColumn and TanStack ColumnDef.
 */
export type CrmDataTableColumnMapping = Readonly<{
  source: CrmRecordsTableColumn;
  target: unknown;
}>;

/**
 * Filter mapping descriptor used to preserve CrmRecordsTable's "all" sentinel semantics.
 */
export type CrmDataTableFilterMapping = Readonly<{
  key: string;
  label: string;
  allLabel: string;
  options: Array<{
    value: string;
    label: string;
  }>;
}>;
