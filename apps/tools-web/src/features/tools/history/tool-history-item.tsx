import Link from "next/link";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Text,
} from "@netmetric/ui";

import type { ToolRunSummaryResponse } from "@/lib/tools-api";
import { tTools } from "@/lib/i18n/tools-i18n";

import { ToolHistoryDeleteForm } from "./tool-history-delete-form";

type ToolHistoryItemProps = {
  item: ToolRunSummaryResponse;
  locale?: string | null | undefined;
};

export function ToolHistoryItem({ item, locale }: ToolHistoryItemProps) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between gap-2">
          <CardTitle className="text-lg">{item.toolSlug}</CardTitle>
          <Badge variant="outline">
            {tTools("tools.history.artifactCount", locale, { count: item.artifactCount })}
          </Badge>
        </div>
        <CardDescription>
          {tTools("tools.history.createdAt", locale, {
            date: new Date(item.createdAtUtc).toLocaleString(),
          })}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <div className="flex flex-wrap gap-2">
          <Button asChild size="sm">
            <Link href={`/history/${item.runId}`}>
              {tTools("tools.actions.viewDetails", locale)}
            </Link>
          </Button>
          <Button asChild variant="outline" size="sm">
            <Link href={`/api/tools/history/${item.runId}/download`}>
              {tTools("tools.actions.downloadLatest", locale)}
            </Link>
          </Button>
        </div>

        <details>
          <summary className="cursor-pointer text-sm text-muted-foreground">
            {tTools("tools.history.deleteThisRun", locale)}
          </summary>
          <div className="mt-3 rounded-sm border p-3">
            <ToolHistoryDeleteForm runId={item.runId} locale={locale} />
          </div>
        </details>

        <Text className="text-xs text-muted-foreground">
          {tTools("tools.history.runId", locale, { id: item.runId })}
        </Text>
      </CardContent>
    </Card>
  );
}
