import type { MetadataRoute } from "next";

import { getToolsCatalog } from "@/features/tools/catalog/catalog-api";
import { getToolRoutePath } from "@/features/tools/catalog/catalog-routes";
import { toolsEnv } from "@/lib/tools-env";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const catalog = await getToolsCatalog();
  const now = new Date();

  const staticPaths = ["/", "/categories", "/privacy", "/terms"];
  const categoryPaths = catalog.categories.map((category) => `/categories/${category.slug}`);
  const enabledToolPaths = catalog.tools
    .filter((tool) => tool.isEnabled)
    .map((tool) => getToolRoutePath(tool.slug));

  const uniquePaths = Array.from(new Set([...staticPaths, ...categoryPaths, ...enabledToolPaths]));

  return uniquePaths.map((path) => ({
    url: `${toolsEnv.siteUrl}${path}`,
    lastModified: now,
    changeFrequency: path === "/" ? "weekly" : "monthly",
    priority: path === "/" ? 1 : 0.7,
  }));
}
