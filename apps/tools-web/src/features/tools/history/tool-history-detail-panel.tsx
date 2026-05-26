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

import type { ToolRunDetailResponse } from "@/lib/tools-api";
import { tTools } from "@/lib/i18n/tools-i18n";

import { ToolHistoryDeleteForm } from "./tool-history-delete-form";

type ToolHistoryDetailPanelProps = {
  run: ToolRunDetailResponse;
  locale?: string | null | undefined;
};

function parseSummary(summary: string): Record<string, string> {
  try {
    const value = JSON.parse(summary) as unknown;
    if (!value || typeof value !== "object" || Array.isArray(value)) {
      return {};
    }

    return Object.fromEntries(
      Object.entries(value).map(([key, raw]) => [key, raw === null ? "null" : String(raw)]),
    );
  } catch {
    return {};
  }
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function ToolHistoryDetailPanel({ run, locale }: ToolHistoryDetailPanelProps) {
  const summaryEntries = Object.entries(parseSummary(run.inputSummaryJson));

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-xl">{run.toolSlug}</CardTitle>
        <CardDescription>
          {tTools("tools.history.createdAt", locale, {
            date: new Date(run.createdAtUtc).toLocaleString(),
          })}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <section className="space-y-2">
          <h2 className="text-sm font-semibold">{tTools("tools.history.inputSummary", locale)}</h2>
          {summaryEntries.length > 0 ? (
            <ul className="space-y-1 rounded-sm border bg-muted p-3 text-xs">
              {summaryEntries.map(([key, value]) => (
                <li key={key}>
                  <span className="font-medium">{key}:</span> {value}
                </li>
              ))}
            </ul>
          ) : (
            <p className="rounded-sm border bg-muted p-3 text-xs text-muted-foreground">
              {tTools("tools.history.summaryUnavailable", locale)}
            </p>
          )}
        </section>

        <section className="space-y-3">
          <h2 className="text-sm font-semibold">{tTools("tools.history.artifacts", locale)}</h2>
          {run.artifacts.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              {tTools("tools.history.noArtifacts", locale)}
            </p>
          ) : (
            <div className="space-y-3">
              {run.artifacts.map((artifact) => (
                <div key={artifact.artifactId} className="rounded-sm border p-3">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <p className="text-sm font-medium">{artifact.fileName}</p>
                    <Badge variant="outline">{artifact.mimeType}</Badge>
                  </div>
                  <p className="text-xs text-muted-foreground">
                    {tTools("tools.history.artifactSize", locale, {
                      size: formatBytes(artifact.sizeBytes),
                    })}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {tTools("tools.history.artifactCreatedAt", locale, {
                      date: new Date(artifact.createdAtUtc).toLocaleString(),
                    })}
                  </p>
                </div>
              ))}
            </div>
          )}
        </section>

        <div className="flex flex-wrap gap-2">
          <Button asChild>
            <Link href={`/api/tools/history/${run.runId}/download`}>
              {tTools("tools.actions.downloadLatest", locale)}
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/history">{tTools("tools.actions.backToHistory", locale)}</Link>
          </Button>
        </div>

        <details>
          <summary className="cursor-pointer text-sm text-muted-foreground">
            {tTools("tools.history.deleteThisRun", locale)}
          </summary>
          <div className="mt-3 rounded-sm border p-3">
            <ToolHistoryDeleteForm runId={run.runId} locale={locale} />
          </div>
        </details>
      </CardContent>
    </Card>
  );
}
