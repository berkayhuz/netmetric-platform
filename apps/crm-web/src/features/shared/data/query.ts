import "server-only";

import type { CrmListQuery, CrmPagedResult } from "@/lib/crm-api";

export function toListQuery(
  searchParams: Record<string, string | string[] | undefined>,
): CrmListQuery {
  const pageRaw = Array.isArray(searchParams.page) ? searchParams.page[0] : searchParams.page;
  const pageSizeRaw = Array.isArray(searchParams.pageSize)
    ? searchParams.pageSize[0]
    : searchParams.pageSize;
  const searchRaw = Array.isArray(searchParams.search)
    ? searchParams.search[0]
    : searchParams.search;
  const sortByRaw = Array.isArray(searchParams.sortBy)
    ? searchParams.sortBy[0]
    : searchParams.sortBy;
  const sortDirectionRaw = Array.isArray(searchParams.sortDirection)
    ? searchParams.sortDirection[0]
    : searchParams.sortDirection;

  const page = pageRaw ? Number(pageRaw) : undefined;
  const pageSize = pageSizeRaw ? Number(pageSizeRaw) : undefined;
  const search = searchRaw?.trim() || undefined;
  const sortBy = sortByRaw?.trim() || undefined;
  const sortDirection =
    sortDirectionRaw === "asc" || sortDirectionRaw === "desc" ? sortDirectionRaw : undefined;

  return {
    ...(page !== undefined ? { page } : {}),
    ...(pageSize !== undefined ? { pageSize } : {}),
    ...(search ? { search } : {}),
    ...(sortBy ? { sortBy } : {}),
    ...(sortDirection ? { sortDirection } : {}),
  };
}

export function emptyPagedResult<T>(page = 1, pageSize = 20): CrmPagedResult<T> {
  return {
    items: [],
    totalCount: 0,
    pageNumber: page,
    pageSize,
    totalPages: 0,
  };
}
