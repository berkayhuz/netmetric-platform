"use client";

import { useEffect, useMemo, useState } from "react";
import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import { SaveToHistoryPanel } from "@/features/tools/history/save-to-history-panel";
import { tTools } from "@/lib/i18n/tools-i18n";

import { convertImageWithCanvas } from "./canvas-conversion";
import { ImageDownloadActions } from "./image-download-actions";
import { downloadConvertedImage } from "./image-download";
import { ImagePreviewPanel } from "./image-preview-panel";
import { ImageUploadPanel } from "./image-upload-panel";
import { MAX_IMAGE_PIXELS, validateInputFile, type ImageConverterMode } from "./image-validation";

type ConvertedOutput = {
  blob: Blob;
  fileName: string;
};

type ImageConverterClientProps = {
  mode: ImageConverterMode;
  isAuthenticated: boolean;
  locale?: string | null | undefined;
};

export function ImageConverterClient({ mode, isAuthenticated, locale }: ImageConverterClientProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [jpgQuality, setJpgQuality] = useState(75);
  const [isPending, setIsPending] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [convertedPreviewUrl, setConvertedPreviewUrl] = useState<string | null>(null);
  const [convertedOutput, setConvertedOutput] = useState<ConvertedOutput | null>(null);
  const [dimensions, setDimensions] = useState<{ width: number; height: number } | null>(null);

  const validation = useMemo(
    () => validateInputFile(selectedFile, mode, locale),
    [mode, selectedFile, locale],
  );

  const accept = mode === "png-to-jpg" ? "image/png" : "image/jpeg";

  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }

      if (convertedPreviewUrl) {
        URL.revokeObjectURL(convertedPreviewUrl);
      }
    };
  }, [convertedPreviewUrl, previewUrl]);

  function handleFileChange(file: File | null): void {
    const nextValidation = validateInputFile(file, mode, locale);
    const nextPreviewUrl = file && nextValidation.isValid ? URL.createObjectURL(file) : null;

    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }

    if (convertedPreviewUrl) {
      URL.revokeObjectURL(convertedPreviewUrl);
      setConvertedPreviewUrl(null);
    }

    setSelectedFile(file);
    setPreviewUrl(nextPreviewUrl);
    setConvertedOutput(null);
    setErrorMessage(nextValidation.isValid || file === null ? null : nextValidation.errorMessage);
    setDimensions(null);

    if (file && nextValidation.isValid) {
      createImageBitmap(file)
        .then((bitmap) => {
          setDimensions({ width: bitmap.width, height: bitmap.height });
          bitmap.close();
        })
        .catch(() => {
          setDimensions(null);
        });
    }
  }

  async function handleConvert(): Promise<void> {
    if (!selectedFile || !validation.isValid) {
      setErrorMessage(
        validation.isValid
          ? tTools("tools.image.validation.selectValidFirst", locale)
          : validation.errorMessage,
      );
      return;
    }

    setIsPending(true);
    setErrorMessage(null);

    try {
      const bitmap = await createImageBitmap(selectedFile);

      if (bitmap.width * bitmap.height > MAX_IMAGE_PIXELS) {
        bitmap.close();
        setErrorMessage(tTools("tools.image.validation.tooLargeDimensions", locale));
        setIsPending(false);
        return;
      }

      const converted = await convertImageWithCanvas({
        imageBitmap: bitmap,
        mode,
        outputFileName: validation.sanitizedFileName,
        jpegQuality: jpgQuality / 100,
      });

      bitmap.close();

      if (convertedPreviewUrl) {
        URL.revokeObjectURL(convertedPreviewUrl);
      }

      const outputPreviewUrl = URL.createObjectURL(converted.blob);

      setConvertedOutput({
        blob: converted.blob,
        fileName: converted.fileName,
      });
      setConvertedPreviewUrl(outputPreviewUrl);
    } catch {
      setErrorMessage(tTools("tools.image.validation.conversionFailed", locale));
    } finally {
      setIsPending(false);
    }
  }

  function handleDownload(): void {
    if (!convertedOutput) {
      return;
    }

    downloadConvertedImage(convertedOutput.blob, convertedOutput.fileName);
  }

  return (
    <section className="mx-auto w-full max-w-4xl px-4 pb-10 sm:px-6 lg:px-8">
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <ImageUploadPanel
          mode={mode}
          accept={accept}
          isPending={isPending}
          errorMessage={
            errorMessage ?? (!validation.isValid && selectedFile ? validation.errorMessage : null)
          }
          onFileChange={handleFileChange}
          jpgQuality={jpgQuality}
          onJpgQualityChange={setJpgQuality}
          locale={locale}
        />
        <ImagePreviewPanel
          previewUrl={convertedPreviewUrl ?? previewUrl}
          width={dimensions?.width ?? null}
          height={dimensions?.height ?? null}
          altText={
            convertedPreviewUrl
              ? tTools("tools.image.preview.convertedAlt", locale)
              : tTools("tools.image.preview.sourceAlt", locale)
          }
          locale={locale}
        />
      </div>

      <ImageDownloadActions
        canConvert={validation.isValid}
        isPending={isPending}
        canDownload={convertedOutput !== null}
        outputFileName={
          convertedOutput?.fileName ??
          (validation.isValid ? validation.sanitizedFileName : "netmetric-converted")
        }
        locale={locale}
        onConvert={() => void handleConvert()}
        onDownload={handleDownload}
      />

      <Alert className="mt-6">
        <AlertTitle>{tTools("tools.image.browserOnly.title", locale)}</AlertTitle>
        <AlertDescription>{tTools("tools.image.browserOnly.description", locale)}</AlertDescription>
      </Alert>

      <SaveToHistoryPanel
        toolSlug={mode}
        isAuthenticated={isAuthenticated}
        canSave={convertedOutput !== null}
        locale={locale}
        getPayload={async () => {
          if (!convertedOutput || !validation.isValid) {
            return null;
          }

          const outputFile = new File([convertedOutput.blob], convertedOutput.fileName, {
            type: mode === "png-to-jpg" ? "image/jpeg" : "image/png",
          });

          return {
            outputFile,
            inputSummaryJson: JSON.stringify({
              sourceMimeType: selectedFile?.type ?? null,
              sourceBytes: selectedFile?.size ?? null,
              outputMimeType: outputFile.type,
              outputBytes: outputFile.size,
              jpgQuality: mode === "png-to-jpg" ? jpgQuality : undefined,
            }),
          };
        }}
      />
    </section>
  );
}
