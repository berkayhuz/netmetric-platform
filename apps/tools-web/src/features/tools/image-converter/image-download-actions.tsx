import { Button, Card, CardContent } from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";

type ImageDownloadActionsProps = {
  canConvert: boolean;
  isPending: boolean;
  canDownload: boolean;
  outputFileName: string;
  locale?: string | null | undefined;
  onConvert: () => void;
  onDownload: () => void;
};

export function ImageDownloadActions({
  canConvert,
  isPending,
  canDownload,
  outputFileName,
  locale,
  onConvert,
  onDownload,
}: ImageDownloadActionsProps) {
  return (
    <Card className="mt-6">
      <CardContent className="flex flex-wrap items-center gap-3 pt-6">
        <Button type="button" onClick={onConvert} disabled={!canConvert || isPending}>
          {isPending
            ? tTools("tools.actions.converting", locale)
            : tTools("tools.actions.convertImage", locale)}
        </Button>
        <Button
          type="button"
          variant="outline"
          onClick={onDownload}
          disabled={!canDownload || isPending}
        >
          {tTools("tools.actions.downloadOutput", locale)}
        </Button>
        <p className="text-sm text-muted-foreground">
          {tTools("tools.image.download.outputFile", locale, { fileName: outputFileName })}
        </p>
        <p className="sr-only" role="status" aria-live="polite">
          {isPending
            ? tTools("tools.image.download.inProgress", locale)
            : tTools("tools.image.download.ready", locale)}
        </p>
      </CardContent>
    </Card>
  );
}
