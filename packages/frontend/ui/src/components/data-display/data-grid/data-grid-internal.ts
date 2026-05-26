import type { DataGridMode } from "./data-grid-types";
import type { PaginationState } from "@tanstack/react-table";

const DATA_GRID_DEFAULT_PAGE_SIZE = 10;

export function getPaginationDefaults(state?: PaginationState): PaginationState {
  return {
    pageIndex: state?.pageIndex ?? 0,
    pageSize: state?.pageSize ?? DATA_GRID_DEFAULT_PAGE_SIZE,
  };
}

export function toErrorMessage(error: string | null | undefined): string | null {
  if (typeof error !== "string") {
    return null;
  }

  const message = error.trim();
  return message.length > 0 ? message : null;
}

export function resolveTotalRows(
  mode: DataGridMode,
  totalRows: number | undefined,
  fallbackLength: number,
): number {
  if (mode === "server") {
    return typeof totalRows === "number" && totalRows >= 0 ? totalRows : fallbackLength;
  }

  return fallbackLength;
}
