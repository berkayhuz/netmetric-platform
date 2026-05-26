import { tTools } from "@/lib/i18n/tools-i18n";

import type { ToolCatalog, ToolCatalogItem, ToolCategory } from "./catalog-types";

function translateWithFallback(
  key: string,
  fallback: string,
  locale?: string | null | undefined,
): string {
  const translated = tTools(key, locale);
  return translated === key ? fallback : translated;
}

export function getToolTitle(tool: ToolCatalogItem, locale?: string | null | undefined): string {
  return translateWithFallback(`tools.catalog.items.${tool.slug}.title`, tool.title, locale);
}

export function getToolDescription(
  tool: ToolCatalogItem,
  locale?: string | null | undefined,
): string {
  return translateWithFallback(
    `tools.catalog.items.${tool.slug}.description`,
    tool.description,
    locale,
  );
}

export function getToolSeoTitle(tool: ToolCatalogItem, locale?: string | null | undefined): string {
  return translateWithFallback(`tools.catalog.items.${tool.slug}.seoTitle`, tool.seoTitle, locale);
}

export function getToolSeoDescription(
  tool: ToolCatalogItem,
  locale?: string | null | undefined,
): string {
  return translateWithFallback(
    `tools.catalog.items.${tool.slug}.seoDescription`,
    tool.seoDescription,
    locale,
  );
}

export function getCategoryTitle(
  category: ToolCategory,
  locale?: string | null | undefined,
): string {
  return translateWithFallback(
    `tools.catalog.categories.${category.slug}.title`,
    category.title,
    locale,
  );
}

export function getCategoryDescription(
  category: ToolCategory,
  locale?: string | null | undefined,
): string {
  return translateWithFallback(
    `tools.catalog.categories.${category.slug}.description`,
    category.description,
    locale,
  );
}

export function localizeToolCatalog(
  catalog: ToolCatalog,
  locale?: string | null | undefined,
): ToolCatalog {
  return {
    categories: catalog.categories.map((category) => ({
      ...category,
      title: getCategoryTitle(category, locale),
      description: getCategoryDescription(category, locale),
    })),
    tools: catalog.tools.map((tool) => ({
      ...tool,
      title: getToolTitle(tool, locale),
      description: getToolDescription(tool, locale),
      seoTitle: getToolSeoTitle(tool, locale),
      seoDescription: getToolSeoDescription(tool, locale),
    })),
  };
}
