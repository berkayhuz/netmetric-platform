import { Button } from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";

type QrDownloadActionsProps = {
  canDownload: boolean;
  locale?: string | null | undefined;
  onDownload: () => void;
};

export function QrDownloadActions({ canDownload, locale, onDownload }: QrDownloadActionsProps) {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Button type="button" onClick={onDownload} disabled={!canDownload}>
        {tTools("tools.actions.downloadPng", locale)}
      </Button>
      <p className="text-sm text-muted-foreground">
        {tTools("tools.qr.download.fileName", locale)}
      </p>
      <p className="sr-only" role="status" aria-live="polite">
        {canDownload
          ? tTools("tools.qr.download.ready", locale)
          : tTools("tools.qr.download.notReady", locale)}
      </p>
    </div>
  );
}
