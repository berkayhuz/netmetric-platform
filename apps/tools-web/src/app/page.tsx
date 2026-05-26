import type { Metadata } from "next";
import Script from "next/script";
import { Code, Heading, Input, Text } from "@netmetric/ui";

import { getToolsCatalog } from "@/features/tools/catalog/catalog-api";
import { localizeToolCatalog } from "@/features/tools/catalog/catalog-i18n";
import { ToolCatalogGrid } from "@/features/tools/components/tool-catalog-grid";
import { ToolsHero } from "@/features/tools/components/tools-hero";
import { ToolsShell } from "@/features/tools/components/tools-shell";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { createPageMetadata } from "@/lib/seo";
import { toAbsoluteUrl } from "@/lib/tools-env";

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return createPageMetadata(
    tTools("tools.home.metaTitle", locale),
    tTools("tools.home.metaDescription", locale),
    "/",
  );
}

export default async function Home({ searchParams }: { searchParams: Promise<{ q?: string }> }) {
  const locale = await getRequestLocale();
  const { q } = await searchParams;
  const catalog = localizeToolCatalog(await getToolsCatalog(), locale);
  const query = q?.trim().toLowerCase() ?? "";

  const tools = query
    ? catalog.tools.filter((tool) => {
        const haystack = `${tool.title} ${tool.description} ${tool.slug}`.toLowerCase();
        return haystack.includes(query);
      })
    : catalog.tools;

  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "WebSite",
    name: "NetMetric Tools",
    url: toAbsoluteUrl("/"),
    potentialAction: {
      "@type": "SearchAction",
      target: `${toAbsoluteUrl("/")}?q={search_term_string}`,
      "query-input": "required name=search_term_string",
    },
  };

  return (
    <>
      <Script id="tools-home-jsonld" type="application/ld+json">
        {JSON.stringify(jsonLd)}
      </Script>
      <ToolsHero locale={locale} />
      <ToolsShell>
        <section aria-labelledby="catalog-heading" className="space-y-4">
          <div className="space-y-2">
            <Heading id="catalog-heading" level={1}>
              {tTools("tools.home.catalogTitle", locale)}
            </Heading>
            <Text className="text-muted-foreground">
              {tTools("tools.home.catalogDescription", locale)}
            </Text>
          </div>
          <form action="/" method="get" className="flex max-w-xl items-center gap-2" role="search">
            <label htmlFor="catalog-search" className="sr-only">
              {tTools("tools.home.searchLabel", locale)}
            </label>
            <Input
              id="catalog-search"
              name="q"
              defaultValue={q ?? ""}
              placeholder={tTools("tools.home.searchPlaceholder", locale)}
            />
          </form>
          {query ? (
            <Text className="text-muted-foreground">
              {tTools("tools.home.showingResultsFor", locale)} <Code>{query}</Code>
            </Text>
          ) : null}
          <ToolCatalogGrid tools={tools} locale={locale} />
        </section>
      </ToolsShell>
    </>
  );
}
