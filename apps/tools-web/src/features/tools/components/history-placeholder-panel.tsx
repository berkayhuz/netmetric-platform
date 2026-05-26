import Link from "next/link";
import {
  Alert,
  AlertDescription,
  AlertTitle,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";
import { toolsEnv } from "@/lib/tools-env";

type HistoryPlaceholderPanelProps = {
  runId?: string;
  locale?: string | null | undefined;
};

export function HistoryPlaceholderPanel({ runId, locale }: HistoryPlaceholderPanelProps) {
  return (
    <section className="mx-auto w-full max-w-4xl px-4 py-10 sm:px-6 lg:px-8">
      <Card>
        <CardHeader>
          <CardTitle>{tTools("tools.history.placeholder.title", locale)}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert>
            <AlertTitle>{tTools("tools.history.placeholder.alertTitle", locale)}</AlertTitle>
            <AlertDescription>
              {tTools("tools.history.placeholder.alertDescription", locale)}
            </AlertDescription>
          </Alert>
          {runId ? (
            <p className="text-sm text-muted-foreground">
              {tTools("tools.history.placeholder.requestedEntry", locale, { id: runId })}
            </p>
          ) : null}
          <div className="flex flex-wrap gap-3">
            <Button asChild>
              <Link href={toolsEnv.authUrl}>{tTools("tools.actions.signIn", locale)}</Link>
            </Button>
            <Button asChild variant="outline">
              <Link href="/">{tTools("tools.actions.backToCatalog", locale)}</Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </section>
  );
}
