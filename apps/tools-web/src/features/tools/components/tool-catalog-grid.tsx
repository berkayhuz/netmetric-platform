import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from "@netmetric/ui";

import type { ToolCatalogItem } from "@/features/tools/catalog/catalog-types";
import { tTools } from "@/lib/i18n/tools-i18n";

import { ToolCard } from "./tool-card";

type ToolCatalogGridProps = {
  tools: ToolCatalogItem[];
  locale?: string | null | undefined;
};

export function ToolCatalogGrid({ tools, locale }: ToolCatalogGridProps) {
  if (tools.length === 0) {
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>{tTools("tools.catalog.emptyTitle", locale)}</EmptyTitle>
          <EmptyDescription>{tTools("tools.catalog.emptyDescription", locale)}</EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
      {tools.map((tool) => (
        <ToolCard key={tool.slug} tool={tool} locale={locale ?? null} />
      ))}
    </div>
  );
}
