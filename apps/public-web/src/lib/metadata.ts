import type { Metadata } from "next";

import { tPublic } from "@/lib/i18n/public-i18n";
import { toAbsoluteUrl } from "@/lib/public-env";

const defaultTitle = "NetMetric";

export const siteTitle = defaultTitle;

export function createPageMetadata(input: {
  title: string;
  description: string;
  path: string;
}): Metadata {
  const canonical = toAbsoluteUrl(input.path);

  return {
    title: input.title,
    description: input.description,
    alternates: {
      canonical,
    },
    openGraph: {
      title: input.title,
      description: input.description,
      url: canonical,
      siteName: defaultTitle,
      type: "website",
    },
    twitter: {
      card: "summary_large_image",
      title: input.title,
      description: input.description,
    },
  };
}

export function defaultSiteDescription(locale?: string | null): string {
  return tPublic("public.meta.defaultDescription", locale);
}
