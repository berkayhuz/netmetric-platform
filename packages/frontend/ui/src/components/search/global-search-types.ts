"use client";

export type GlobalSearchSourceGroup = "crm" | "account" | "tools" | "public" | "other";

export type GlobalSearchSourceValue =
  | GlobalSearchSourceGroup
  | "auth"
  | "forum"
  | "admin"
  | "platform";

export type GlobalSearchResultItem = {
  id: string;
  source: string | number;
  type: string;
  title: string;
  summary?: string | null;
  url: string;
  visibility?: string | number;
  locale?: string | null;
  tags?: readonly string[] | null;
  rankingScore?: number | null;
  highlightedTitle?: string | null;
  highlightedSummary?: string | null;
  highlights?: readonly unknown[] | null;
  [key: string]: unknown;
};

export type GlobalSearchResponse = {
  query?: string;
  page?: number;
  pageSize?: number;
  totalCount?: number;
  items?: readonly GlobalSearchResultItem[];
  permissionPostFilteringApplied?: boolean;
};

export type GlobalSearchGroup = {
  source: GlobalSearchSourceGroup;
  label: string;
  items: readonly GlobalSearchResultItem[];
};
