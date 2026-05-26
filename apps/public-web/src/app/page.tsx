import type { Metadata } from "next";

import { HomeHero } from "@/features/public/components/home-hero";
import { getPageContent, type PublicPageKey } from "@/features/public/content/pages";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tPublic } from "@/lib/i18n/public-i18n";
import { createPageMetadata } from "@/lib/metadata";

const pageKey: PublicPageKey = "home";
const pagePath = "/";

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();
  const content = getPageContent(pageKey, locale);

  return createPageMetadata({
    title: tPublic(`public.pages.${pageKey}.metaTitle`, locale),
    description: content.description,
    path: pagePath,
  });
}

export default async function HomePage() {
  const locale = await getRequestLocale();
  const content = getPageContent(pageKey, locale);

  return <HomeHero content={content} locale={locale} />;
}
