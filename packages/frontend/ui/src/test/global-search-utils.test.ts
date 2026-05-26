import { describe, expect, it } from "vitest";

import {
  groupGlobalSearchResults,
  isSafeGlobalSearchUrl,
  normalizeGlobalSearchSource,
  normalizeGlobalSearchSourceValue,
  resolveGlobalSearchLocale,
  toGlobalSearchSourceQueryValue,
} from "../client";

import type { GlobalSearchResultItem } from "../client";

describe("global search utilities", () => {
  it("normalizes string and numeric backend source values", () => {
    expect(normalizeGlobalSearchSource("Crm")).toBe("crm");
    expect(normalizeGlobalSearchSource(3)).toBe("account");
    expect(normalizeGlobalSearchSource("2")).toBe("tools");
    expect(normalizeGlobalSearchSource("Admin")).toBe("other");
    expect(normalizeGlobalSearchSourceValue(5)).toBe("crm");
    expect(toGlobalSearchSourceQueryValue("public")).toBe("Public");
  });

  it("groups results using the requested source order", () => {
    const items: GlobalSearchResultItem[] = [
      {
        id: "public-1",
        source: "Public",
        type: "page",
        title: "Pricing",
        url: "/pricing",
      },
      {
        id: "crm-1",
        source: 5,
        type: "customer",
        title: "Customer",
        url: "/crm/customers/1",
      },
    ];

    const groups = groupGlobalSearchResults(items, ["public", "crm", "account", "tools", "other"]);

    expect(groups.map((group) => group.source)).toEqual(["public", "crm"]);
    expect(groups[0]?.items[0]?.title).toBe("Pricing");
  });

  it("accepts only relative application URLs for navigation", () => {
    expect(isSafeGlobalSearchUrl("/crm/customers/1")).toBe(true);
    expect(isSafeGlobalSearchUrl("//example.com/path")).toBe(false);
    expect(isSafeGlobalSearchUrl("javascript:alert(1)")).toBe(false);
    expect(isSafeGlobalSearchUrl("https://example.com")).toBe(false);
  });

  it("canonicalizes supported search locales", () => {
    expect(resolveGlobalSearchLocale("tr")).toBe("tr-TR");
    expect(resolveGlobalSearchLocale("tr-tr")).toBe("tr-TR");
    expect(resolveGlobalSearchLocale("en")).toBe("en-US");
    expect(resolveGlobalSearchLocale("unknown")).toBe("en-US");
  });
});
