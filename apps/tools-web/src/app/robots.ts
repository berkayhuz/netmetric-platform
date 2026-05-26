import type { MetadataRoute } from "next";

import { toolsEnv } from "@/lib/tools-env";

export default function robots(): MetadataRoute.Robots {
  return {
    rules: [
      {
        userAgent: "*",
        allow: "/",
      },
      {
        userAgent: "*",
        disallow: ["/history", "/history/*"],
      },
    ],
    sitemap: `${toolsEnv.siteUrl}/sitemap.xml`,
    host: toolsEnv.siteUrl,
  };
}
