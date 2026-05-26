import type { MetadataRoute } from "next";

import { publicEnv } from "@/lib/public-env";

const paths = [
  "/",
  "/product",
  "/crm",
  "/tools",
  "/developers",
  "/security",
  "/pricing",
  "/about",
  "/contact",
  "/status",
  "/privacy",
  "/terms",
  "/cookies",
] as const;

export default function sitemap(): MetadataRoute.Sitemap {
  const now = new Date();
  return paths.map((path) => ({
    url: `${publicEnv.siteUrl}${path}`,
    lastModified: now,
    changeFrequency: path === "/" ? "weekly" : "monthly",
    priority: path === "/" ? 1 : 0.7,
  }));
}
