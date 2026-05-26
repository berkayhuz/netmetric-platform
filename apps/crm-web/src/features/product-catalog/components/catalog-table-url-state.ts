"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useTransition } from "react";

import type {
  DataTableColumnFiltersState,
  DataTablePaginationState,
  DataTableSortingState,
} from "@netmetric/ui/client";

type FilterParamMap = Record<string, string>;
type ReadableSearchParams = Pick<URLSearchParams, "get">;

function toStringArray(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.filter((item): item is string => typeof item === "string" && item.length > 0);
  }

  if (typeof value === "string" && value.length > 0) {
    return [value];
  }

  if (typeof value === "number" || typeof value === "boolean") {
    return [String(value)];
  }

  return [];
}

function setOrDelete(params: URLSearchParams, key: string, value: string | null): void {
  const normalized = value?.trim();
  if (normalized) {
    params.set(key, normalized);
    return;
  }

  params.delete(key);
}

export function getCatalogTableSorting(searchParams: ReadableSearchParams): DataTableSortingState {
  const sortBy = searchParams.get("sortBy");
  const sortDirection = searchParams.get("sortDirection");

  return sortBy ? [{ id: sortBy, desc: sortDirection === "desc" }] : [];
}

export function getCatalogTablePagination(
  pageNumber: number,
  pageSize: number,
): DataTablePaginationState {
  return {
    pageIndex: Math.max(0, pageNumber - 1),
    pageSize,
  };
}

export function getCatalogTableColumnFilters(
  searchParams: ReadableSearchParams,
  filterParamByColumnId: FilterParamMap,
): DataTableColumnFiltersState {
  return Object.entries(filterParamByColumnId).flatMap(([columnId, paramName]) => {
    const value = searchParams.get(paramName);
    return value ? [{ id: columnId, value: [value] }] : [];
  });
}

export function useCatalogTableUrlState(filterParamByColumnId: FilterParamMap) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();

  function replace(mutator: (params: URLSearchParams) => void): void {
    const params = new URLSearchParams(searchParams.toString());
    mutator(params);

    const query = params.toString();
    const nextUrl = query ? `${pathname}?${query}` : pathname;
    startTransition(() => router.replace(nextUrl, { scroll: false }));
  }

  function resetPage(params: URLSearchParams): void {
    params.set("page", "1");
  }

  return {
    isPending,
    searchParams,
    refresh: () => startTransition(() => router.refresh()),
    updateGlobalFilter: (value: string) =>
      replace((params) => {
        setOrDelete(params, "search", value);
        resetPage(params);
      }),
    updateSorting: (sorting: DataTableSortingState) =>
      replace((params) => {
        const next = sorting[0];
        if (next) {
          params.set("sortBy", next.id);
          params.set("sortDirection", next.desc ? "desc" : "asc");
        } else {
          params.delete("sortBy");
          params.delete("sortDirection");
        }
        resetPage(params);
      }),
    updatePagination: (pagination: DataTablePaginationState) =>
      replace((params) => {
        params.set("page", String(pagination.pageIndex + 1));
        params.set("pageSize", String(pagination.pageSize));
      }),
    updateColumnFilters: (filters: DataTableColumnFiltersState) =>
      replace((params) => {
        for (const paramName of Object.values(filterParamByColumnId)) {
          params.delete(paramName);
        }

        for (const filter of filters) {
          const paramName = filterParamByColumnId[filter.id] ?? filter.id;
          const [value] = toStringArray(filter.value);
          setOrDelete(params, paramName, value ?? null);
        }

        resetPage(params);
      }),
  };
}
