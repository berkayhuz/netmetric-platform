"use client";

import type {
  GlobalSearchGroup,
  GlobalSearchResultItem,
  GlobalSearchSourceGroup,
  GlobalSearchSourceValue,
} from "./global-search-types";

const sourceNameByNumber = new Map<number, GlobalSearchSourceValue>([
  [1, "public"],
  [2, "tools"],
  [3, "account"],
  [4, "auth"],
  [5, "crm"],
  [6, "forum"],
  [7, "admin"],
  [8, "platform"],
]);

const sourceLabels: Record<GlobalSearchSourceGroup, string> = {
  crm: "CRM",
  account: "Account",
  tools: "Tools",
  public: "Public",
  other: "Other",
};

export const defaultGlobalSearchSourceOrder = [
  "crm",
  "account",
  "tools",
  "public",
  "other",
] as const satisfies readonly GlobalSearchSourceGroup[];

export const defaultGlobalSearchLocale = "en-US";

export function resolveGlobalSearchLocale(locale: string | null | undefined): string {
  const normalized = locale?.trim().toLowerCase();
  if (!normalized) {
    return defaultGlobalSearchLocale;
  }

  if (normalized === "tr" || normalized.startsWith("tr-")) {
    return "tr-TR";
  }

  if (normalized === "en" || normalized.startsWith("en-")) {
    return "en-US";
  }

  return defaultGlobalSearchLocale;
}

export function normalizeGlobalSearchSource(source: unknown): GlobalSearchSourceGroup {
  const normalizedValue = normalizeGlobalSearchSourceValue(source);

  if (
    normalizedValue === "crm" ||
    normalizedValue === "account" ||
    normalizedValue === "tools" ||
    normalizedValue === "public"
  ) {
    return normalizedValue;
  }

  return "other";
}

export function normalizeGlobalSearchSourceValue(source: unknown): GlobalSearchSourceValue {
  if (typeof source === "number" && Number.isFinite(source)) {
    return sourceNameByNumber.get(source) ?? "platform";
  }

  const text = String(source ?? "")
    .trim()
    .toLowerCase();

  if (!text) {
    return "platform";
  }

  const numeric = Number(text);
  if (Number.isInteger(numeric)) {
    return sourceNameByNumber.get(numeric) ?? "platform";
  }

  if (
    text === "crm" ||
    text === "account" ||
    text === "tools" ||
    text === "public" ||
    text === "auth" ||
    text === "forum" ||
    text === "admin" ||
    text === "platform"
  ) {
    return text;
  }

  return "platform";
}

export function getGlobalSearchSourceLabel(source: GlobalSearchSourceGroup): string {
  return sourceLabels[source];
}

export function groupGlobalSearchResults(
  items: readonly GlobalSearchResultItem[],
  sourceOrder: readonly GlobalSearchSourceGroup[] = defaultGlobalSearchSourceOrder,
): readonly GlobalSearchGroup[] {
  const buckets = new Map<GlobalSearchSourceGroup, GlobalSearchResultItem[]>();

  for (const item of items) {
    const source = normalizeGlobalSearchSource(item.source);
    const bucket = buckets.get(source) ?? [];
    bucket.push(item);
    buckets.set(source, bucket);
  }

  const orderedSources = [...sourceOrder, ...defaultGlobalSearchSourceOrder].filter(
    (source, index, sources) => sources.indexOf(source) === index,
  );

  return orderedSources
    .map((source) => ({
      source,
      label: getGlobalSearchSourceLabel(source),
      items: buckets.get(source) ?? [],
    }))
    .filter((group) => group.items.length > 0);
}

export function isSafeGlobalSearchUrl(url: unknown): url is string {
  if (typeof url !== "string") {
    return false;
  }

  const trimmed = url.trim();
  return trimmed.startsWith("/") && !trimmed.startsWith("//") && !hasControlCharacter(trimmed);
}

export function toGlobalSearchSourceQueryValue(source: GlobalSearchSourceValue): string {
  return source.charAt(0).toUpperCase() + source.slice(1);
}

function hasControlCharacter(value: string): boolean {
  for (const character of value) {
    if (character.charCodeAt(0) < 32) {
      return true;
    }
  }

  return false;
}
