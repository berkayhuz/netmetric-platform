import "server-only";

import type { CrmListQuery, CrmNormalizedListQuery } from "./crm-api-types";

const DEFAULT_PAGE = 1;
const DEFAULT_PAGE_SIZE = 20;
const MAX_PAGE_SIZE = 500;

function normalizePositiveInt(value: number | undefined, fallback: number): number {
  if (!value || !Number.isFinite(value)) {
    return fallback;
  }

  const normalized = Math.floor(value);
  if (normalized < 1) {
    return fallback;
  }

  return normalized;
}

export function normalizeListQuery(input: CrmListQuery = {}): CrmNormalizedListQuery {
  const page = normalizePositiveInt(input.page, DEFAULT_PAGE);
  const requestedPageSize = normalizePositiveInt(input.pageSize, DEFAULT_PAGE_SIZE);
  const pageSize = Math.min(requestedPageSize, MAX_PAGE_SIZE);

  const filters: Record<string, string | number | boolean> = {};
  for (const [key, value] of Object.entries(input.filters ?? {})) {
    if (value === undefined || value === null) {
      continue;
    }

    filters[key] = value;
  }

  const search = input.search?.trim();
  const sortBy = input.sortBy?.trim();

  return {
    page,
    pageSize,
    ...(search ? { search } : {}),
    ...(sortBy ? { sortBy } : {}),
    ...(input.sortDirection ? { sortDirection: input.sortDirection } : {}),
    filters,
  };
}

export function listQueryToSearchParams(input: CrmListQuery = {}): URLSearchParams {
  const query = normalizeListQuery(input);
  const params = new URLSearchParams();

  params.set("page", String(query.page));
  params.set("pageSize", String(query.pageSize));

  if (query.search) {
    params.set("search", query.search);
  }

  if (query.sortBy) {
    params.set("sortBy", query.sortBy);
  }

  if (query.sortDirection) {
    params.set("sortDirection", query.sortDirection);
  }

  for (const [key, value] of Object.entries(query.filters)) {
    params.set(key, String(value));
  }

  return params;
}
