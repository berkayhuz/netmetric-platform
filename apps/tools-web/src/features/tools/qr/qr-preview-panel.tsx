import Image from "next/image";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";

type QrPreviewPanelProps = {
  previewDataUrl: string | null;
  locale?: string | null | undefined;
};

export function QrPreviewPanel({ previewDataUrl, locale }: QrPreviewPanelProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tTools("tools.qr.preview.title", locale)}</CardTitle>
        <CardDescription>{tTools("tools.qr.preview.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        {previewDataUrl ? (
          <Image
            src={previewDataUrl}
            alt={tTools("tools.qr.preview.alt", locale)}
            width={256}
            height={256}
            unoptimized
            className="mx-auto rounded-sm border bg-white object-contain p-2"
          />
        ) : (
          <p className="rounded-sm border border-dashed p-6 text-center text-sm text-muted-foreground">
            {tTools("tools.qr.preview.empty", locale)}
          </p>
        )}
      </CardContent>
    </Card>
  );
}
