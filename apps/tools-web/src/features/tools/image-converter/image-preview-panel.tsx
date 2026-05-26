import Image from "next/image";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";

type ImagePreviewPanelProps = {
  previewUrl: string | null;
  width: number | null;
  height: number | null;
  altText: string;
  locale?: string | null | undefined;
};

export function ImagePreviewPanel({
  previewUrl,
  width,
  height,
  altText,
  locale,
}: ImagePreviewPanelProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tTools("tools.image.preview.title", locale)}</CardTitle>
        <CardDescription>{tTools("tools.image.preview.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {previewUrl ? (
          <Image
            src={previewUrl}
            alt={altText}
            width={320}
            height={320}
            unoptimized
            className="mx-auto max-h-80 rounded-sm border bg-white object-contain p-1"
          />
        ) : (
          <p className="rounded-sm border border-dashed p-6 text-center text-sm text-muted-foreground">
            {tTools("tools.image.preview.empty", locale)}
          </p>
        )}

        {width && height ? (
          <p className="text-xs text-muted-foreground">
            {tTools("tools.image.preview.dimensions", locale, { width, height })}
          </p>
        ) : null}
      </CardContent>
    </Card>
  );
}
