import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Heading, Text } from "@netmetric/ui";

import { getToolsCatalog } from "@/features/tools/catalog/catalog-api";
import { localizeToolCatalog } from "@/features/tools/catalog/catalog-i18n";
import { ToolCatalogGrid } from "@/features/tools/components/tool-catalog-grid";
import { ToolsShell } from "@/features/tools/components/tools-shell";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { createPageMetadata } from "@/lib/seo";

export async function generateMetadata({
  params,
}: {
  params: Promise<{ category: string }>;
}): Promise<Metadata> {
  const { category } = await params;
  const locale = await getRequestLocale();
  const catalog = localizeToolCatalog(await getToolsCatalog(), locale);
  const found = catalog.categories.find((item) => item.slug === category);

  if (!found) {
    return createPageMetadata(
      tTools("tools.categories.fallbackMetaTitle", locale),
      tTools("tools.categories.fallbackMetaDescription", locale),
      "/categories",
    );
  }

  return createPageMetadata(found.title, found.description, `/categories/${found.slug}`);
}

export default async function CategoryDetailPage({
  params,
}: {
  params: Promise<{ category: string }>;
}) {
  const { category } = await params;
  const locale = await getRequestLocale();
  const catalog = localizeToolCatalog(await getToolsCatalog(), locale);
  const selected = catalog.categories.find((item) => item.slug === category);

  if (!selected) {
    notFound();
  }

  const tools = catalog.tools.filter((tool) => tool.categorySlug === category);

  return (
    <ToolsShell>
      <section aria-labelledby="category-heading" className="space-y-6">
        <div className="space-y-2">
          <Heading id="category-heading" level={1}>
            {selected.title}
          </Heading>
          <Text className="text-muted-foreground">{selected.description}</Text>
        </div>
        <ToolCatalogGrid tools={tools} locale={locale} />
      </section>
    </ToolsShell>
  );
}
