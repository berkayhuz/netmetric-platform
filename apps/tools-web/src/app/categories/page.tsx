import type { Metadata } from "next";
import { Heading, Text } from "@netmetric/ui";

import { getToolsCatalog } from "@/features/tools/catalog/catalog-api";
import { localizeToolCatalog } from "@/features/tools/catalog/catalog-i18n";
import { CategoryCard } from "@/features/tools/components/category-card";
import { ToolsShell } from "@/features/tools/components/tools-shell";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { createPageMetadata } from "@/lib/seo";

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return createPageMetadata(
    tTools("tools.categories.metaTitle", locale),
    tTools("tools.categories.metaDescription", locale),
    "/categories",
  );
}

export default async function CategoriesPage() {
  const locale = await getRequestLocale();
  const catalog = localizeToolCatalog(await getToolsCatalog(), locale);

  const sortedCategories = [...catalog.categories].sort((a, b) => a.sortOrder - b.sortOrder);
  const toolCountsByCategory = catalog.tools.reduce<Record<string, number>>((counts, tool) => {
    counts[tool.categorySlug] = (counts[tool.categorySlug] ?? 0) + 1;
    return counts;
  }, {});

  return (
    <ToolsShell>
      <section aria-labelledby="categories-heading" className="space-y-6">
        <div className="space-y-2">
          <Heading id="categories-heading" level={1}>
            {tTools("tools.categories.title", locale)}
          </Heading>
          <Text className="text-muted-foreground">
            {tTools("tools.categories.description", locale)}
          </Text>
        </div>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {sortedCategories.map((category) => (
            <CategoryCard
              key={category.slug}
              category={category}
              toolCount={toolCountsByCategory[category.slug] ?? 0}
              locale={locale}
            />
          ))}
        </div>
      </section>
    </ToolsShell>
  );
}
