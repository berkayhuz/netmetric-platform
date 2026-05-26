import Link from "next/link";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@netmetric/ui";

import type { ToolCatalogItem } from "@/features/tools/catalog/catalog-types";
import { tTools } from "@/lib/i18n/tools-i18n";

type ToolDetailShellProps = {
  tool: ToolCatalogItem;
  categoryTitle: string;
  isExecutionAvailable?: boolean;
  locale?: string | null | undefined;
};

function toMegabytes(bytes: number): number {
  return Math.round((bytes / 1024 / 1024) * 10) / 10;
}

export function ToolDetailShell({
  tool,
  categoryTitle,
  isExecutionAvailable = false,
  locale,
}: ToolDetailShellProps) {
  return (
    <section className="mx-auto w-full max-w-4xl px-4 py-10 sm:px-6 lg:px-8">
      <Card>
        <CardHeader>
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant="secondary">
              {tool.isEnabled
                ? tTools("tools.status.enabled", locale)
                : tTools("tools.status.comingSoon", locale)}
            </Badge>
            <Badge variant="outline">{tool.executionMode}</Badge>
            <Badge variant="outline">{categoryTitle}</Badge>
          </div>
          <CardTitle className="text-3xl">{tool.title}</CardTitle>
          <CardDescription>{tool.description}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3 text-sm text-muted-foreground">
          <p>
            {tTools("tools.detail.guestLimit", locale, {
              size: toMegabytes(tool.guestMaxFileBytes),
            })}
          </p>
          <p>
            {tTools("tools.detail.authLimit", locale, {
              size: toMegabytes(tool.authenticatedMaxSaveBytes),
            })}
          </p>
          {isExecutionAvailable ? (
            <p>{tTools("tools.detail.browserExecution", locale)}</p>
          ) : (
            <p>{tTools("tools.detail.executionComingSoon", locale)}</p>
          )}
          <p>{tTools("tools.detail.signInHistoryHint", locale)}</p>
          {tool.acceptedMimeTypes.length > 0 ? (
            <p>
              {tTools("tools.detail.acceptedMimeHints", locale, {
                mimeTypes: tool.acceptedMimeTypes.join(", "),
              })}
            </p>
          ) : null}
        </CardContent>
        <div className="px-6 pb-6">
          <Button asChild variant="outline">
            <Link href="/history">{tTools("tools.actions.goToHistory", locale)}</Link>
          </Button>
        </div>
      </Card>
    </section>
  );
}
