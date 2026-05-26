import { fallbackCatalog } from "./catalog-fallback";
import type { ToolCatalog, ToolCatalogItem } from "./catalog-types";
import { toolsEnv } from "@/lib/tools-env";

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function toCatalog(data: unknown): ToolCatalog | null {
  if (!isRecord(data)) {
    return null;
  }

  const categories = data.categories;
  const tools = data.tools;

  if (!Array.isArray(categories) || !Array.isArray(tools)) {
    return null;
  }

  return {
    categories: categories.filter(isRecord).map((category) => ({
      slug: String(category.slug ?? ""),
      title: String(category.title ?? ""),
      description: String(category.description ?? ""),
      sortOrder: Number(category.sortOrder ?? 0),
    })),
    tools: tools.filter(isRecord).map((tool) => ({
      slug: String(tool.slug ?? ""),
      title: String(tool.title ?? ""),
      description: String(tool.description ?? ""),
      categorySlug: String(tool.categorySlug ?? ""),
      executionMode: String(tool.executionMode ?? "future"),
      availabilityStatus: String(tool.availabilityStatus ?? "coming-soon"),
      isEnabled: Boolean(tool.isEnabled),
      acceptedMimeTypes: Array.isArray(tool.acceptedMimeTypes)
        ? tool.acceptedMimeTypes.map((mime) => String(mime))
        : [],
      guestMaxFileBytes: Number(tool.guestMaxFileBytes ?? 0),
      authenticatedMaxSaveBytes: Number(tool.authenticatedMaxSaveBytes ?? 0),
      seoTitle: String(tool.seoTitle ?? ""),
      seoDescription: String(tool.seoDescription ?? ""),
    })),
  };
}

export async function getToolsCatalog(): Promise<ToolCatalog> {
  const endpoint = `${toolsEnv.toolsApiBaseUrl}/api/v1/tools/catalog`;

  try {
    const response = await fetch(endpoint, {
      next: { revalidate: 300 },
      headers: {
        Accept: "application/json",
      },
    });

    if (!response.ok) {
      return fallbackCatalog;
    }

    const parsed = toCatalog(await response.json());
    return parsed ?? fallbackCatalog;
  } catch {
    return fallbackCatalog;
  }
}

export async function getToolDetail(slug: string): Promise<ToolCatalogItem | null> {
  const endpoint = `${toolsEnv.toolsApiBaseUrl}/api/v1/tools/catalog/${encodeURIComponent(slug)}`;

  try {
    const response = await fetch(endpoint, {
      next: { revalidate: 300 },
      headers: {
        Accept: "application/json",
      },
    });

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      return fallbackCatalog.tools.find((tool) => tool.slug === slug) ?? null;
    }

    const parsed = toCatalog({ categories: [], tools: [await response.json()] });
    return parsed?.tools[0] ?? null;
  } catch {
    return fallbackCatalog.tools.find((tool) => tool.slug === slug) ?? null;
  }
}
