import type { Metadata } from "next";
import Script from "next/script";
import { notFound } from "next/navigation";

import { getToolDetail, getToolsCatalog } from "@/features/tools/catalog/catalog-api";
import {
  getToolSeoDescription,
  getToolSeoTitle,
  localizeToolCatalog,
} from "@/features/tools/catalog/catalog-i18n";
import { ToolComingSoonPanel } from "@/features/tools/components/tool-coming-soon-panel";
import { ToolDetailShell } from "@/features/tools/components/tool-detail-shell";
import { ImageConverterClient } from "@/features/tools/image-converter/image-converter-client";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { createPageMetadata } from "@/lib/seo";
import { getToolsAuthStatus } from "@/lib/tools-auth/tools-auth-headers";
import { toAbsoluteUrl } from "@/lib/tools-env";

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();
  const tool = await getToolDetail("jpg-to-png");

  return createPageMetadata(
    tool ? getToolSeoTitle(tool, locale) : "JPG to PNG Converter",
    tool ? getToolSeoDescription(tool, locale) : "JPG to PNG Converter",
    "/image/jpg-to-png",
  );
}

export default async function JpgToPngPage() {
  const locale = await getRequestLocale();
  const tool = await getToolDetail("jpg-to-png");

  if (!tool) {
    notFound();
  }

  const catalog = localizeToolCatalog(await getToolsCatalog(), locale);
  const localizedTool =
    localizeToolCatalog({ categories: [], tools: [tool] }, locale).tools[0] ?? tool;
  const authStatus = await getToolsAuthStatus();
  const categoryTitle =
    catalog.categories.find((category) => category.slug === localizedTool.categorySlug)?.title ??
    localizedTool.categorySlug;

  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "SoftwareApplication",
    name: localizedTool.title,
    applicationCategory: "UtilitiesApplication",
    operatingSystem: "Any",
    description: localizedTool.description,
    url: toAbsoluteUrl("/image/jpg-to-png"),
  };

  return (
    <>
      <Script id="jpg-to-png-jsonld" type="application/ld+json">
        {JSON.stringify(jsonLd)}
      </Script>
      <ToolDetailShell
        tool={localizedTool}
        categoryTitle={categoryTitle}
        isExecutionAvailable
        locale={locale}
      />
      {localizedTool.isEnabled ? (
        <ImageConverterClient
          mode="jpg-to-png"
          isAuthenticated={authStatus.isAuthenticated}
          locale={locale}
        />
      ) : null}
      {!localizedTool.isEnabled ? (
        <div className="mx-auto mb-10 w-full max-w-4xl px-4 sm:px-6 lg:px-8">
          <ToolComingSoonPanel locale={locale} />
        </div>
      ) : null}
    </>
  );
}
