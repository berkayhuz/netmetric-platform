import Link from "next/link";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@netmetric/ui";

import type { ToolCatalogItem } from "@/features/tools/catalog/catalog-types";
import { getToolRoutePath } from "@/features/tools/catalog/catalog-routes";
import { tTools } from "@/lib/i18n/tools-i18n";

type ToolCardProps = {
  tool: ToolCatalogItem;
  locale?: string | null | undefined;
};

function toMegabytes(bytes: number): number {
  return Math.round((bytes / 1024 / 1024) * 10) / 10;
}

export function ToolCard({ tool, locale }: ToolCardProps) {
  const path = getToolRoutePath(tool.slug);
  const isComingSoon = !tool.isEnabled;

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between gap-3">
          <CardTitle>{tool.title}</CardTitle>
          <Badge variant={isComingSoon ? "outline" : "secondary"}>
            {isComingSoon
              ? tTools("tools.status.comingSoon", locale)
              : tTools("tools.status.enabled", locale)}
          </Badge>
        </div>
        <CardDescription>{tool.description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-2 text-sm text-muted-foreground">
        <p>{tTools("tools.card.execution", locale, { mode: tool.executionMode })}</p>
        <p>
          {tTools("tools.card.guestMax", locale, { size: toMegabytes(tool.guestMaxFileBytes) })}
        </p>
        <p>
          {tTools("tools.card.signedInMax", locale, {
            size: toMegabytes(tool.authenticatedMaxSaveBytes),
          })}
        </p>
      </CardContent>
      <CardFooter>
        <Button asChild variant={isComingSoon ? "outline" : "default"}>
          <Link href={path}>
            {isComingSoon
              ? tTools("tools.actions.viewDetails", locale)
              : tTools("tools.actions.openTool", locale)}
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
