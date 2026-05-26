import Link from "next/link";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@netmetric/ui";

import type { ToolCategory } from "@/features/tools/catalog/catalog-types";
import { tTools } from "@/lib/i18n/tools-i18n";

type CategoryCardProps = {
  category: ToolCategory;
  toolCount: number;
  locale?: string | null | undefined;
};

export function CategoryCard({ category, toolCount, locale }: CategoryCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{category.title}</CardTitle>
        <CardDescription>{category.description}</CardDescription>
      </CardHeader>
      <CardContent className="text-sm text-muted-foreground">
        {tTools("tools.categories.toolCount", locale, { count: toolCount })}
      </CardContent>
      <CardFooter>
        <Link
          href={`/categories/${category.slug}`}
          className="text-sm font-medium underline-offset-4 hover:underline"
        >
          {tTools("tools.categories.viewCategory", locale)}
        </Link>
      </CardFooter>
    </Card>
  );
}
