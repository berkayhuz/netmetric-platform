import type { Metadata } from "next";

import { toAbsoluteUrl } from "@/lib/tools-env";

export function createPageMetadata(title: string, description: string, path: string): Metadata {
  const canonical = toAbsoluteUrl(path);

  return {
    title,
    description,
    alternates: {
      canonical,
    },
    openGraph: {
      type: "website",
      title,
      description,
      url: canonical,
      siteName: "NetMetric Tools",
    },
    twitter: {
      card: "summary_large_image",
      title,
      description,
    },
  };
}

export function createNoIndexMetadata(title: string, description: string, path: string): Metadata {
  const metadata = createPageMetadata(title, description, path);
  return {
    ...metadata,
    robots: {
      index: false,
      follow: false,
      nocache: true,
      googleBot: {
        index: false,
        follow: false,
        noarchive: true,
        nosnippet: true,
        noimageindex: true,
      },
    },
  };
}
