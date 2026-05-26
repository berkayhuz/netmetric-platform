import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import { tTools } from "@/lib/i18n/tools-i18n";

import type { ImageConverterMode } from "./image-validation";

type ImageUploadPanelProps = {
  mode: ImageConverterMode;
  accept: string;
  isPending: boolean;
  errorMessage: string | null;
  onFileChange: (file: File | null) => void;
  jpgQuality: number;
  onJpgQualityChange: (quality: number) => void;
  locale?: string | null | undefined;
};

export function ImageUploadPanel({
  mode,
  accept,
  isPending,
  errorMessage,
  onFileChange,
  jpgQuality,
  onJpgQualityChange,
  locale,
}: ImageUploadPanelProps) {
  const describedBy = errorMessage ? "image-file-help image-file-error" : "image-file-help";

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tTools("tools.image.upload.title", locale)}</CardTitle>
        <CardDescription>
          {mode === "png-to-jpg"
            ? tTools("tools.image.upload.pngDescription", locale)
            : tTools("tools.image.upload.jpgDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <label htmlFor="converter-file" className="text-sm font-medium">
            {tTools("tools.image.upload.sourceLabel", locale)}
          </label>
          <input
            id="converter-file"
            type="file"
            accept={accept}
            onChange={(event) => onFileChange(event.target.files?.[0] ?? null)}
            disabled={isPending}
            aria-invalid={Boolean(errorMessage)}
            aria-describedby={describedBy}
            className="block w-full text-sm file:mr-4 file:rounded-sm file:border file:border-input file:bg-background file:px-3 file:py-2 file:text-sm file:font-medium"
          />
          <p id="image-file-help" className="text-xs text-muted-foreground">
            {tTools("tools.image.upload.help", locale)}
          </p>
          {errorMessage ? (
            <p id="image-file-error" role="alert" className="text-sm text-destructive">
              {errorMessage}
            </p>
          ) : null}
        </div>

        {mode === "png-to-jpg" ? (
          <div className="space-y-2">
            <label htmlFor="jpg-quality" className="text-sm font-medium">
              {tTools("tools.image.upload.qualityLabel", locale)}
            </label>
            <Select
              value={String(jpgQuality)}
              onValueChange={(value) => onJpgQualityChange(Number(value))}
            >
              <SelectTrigger id="jpg-quality" disabled={isPending}>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="60">{tTools("tools.image.quality.60", locale)}</SelectItem>
                <SelectItem value="75">{tTools("tools.image.quality.75", locale)}</SelectItem>
                <SelectItem value="90">{tTools("tools.image.quality.90", locale)}</SelectItem>
              </SelectContent>
            </Select>
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}
