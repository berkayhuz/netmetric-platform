"use client";

import { useEffect, useMemo, useState } from "react";
import QRCode from "qrcode";
import { Card, CardContent } from "@netmetric/ui";

import { SaveToHistoryPanel } from "@/features/tools/history/save-to-history-panel";

import { downloadQrPng } from "./qr-download";
import { QrDownloadActions } from "./qr-download-actions";
import { QrInputPanel } from "./qr-input-panel";
import { QrPreviewPanel } from "./qr-preview-panel";
import { validateQrInput } from "./qr-validation";

type QrErrorCorrection = "L" | "M" | "Q" | "H";

const DEFAULT_INPUT = "";
const DEFAULT_QR_SIZE = 256;
const DEFAULT_QR_ECL: QrErrorCorrection = "M";

const sizeOptions = [
  { value: 192, label: "192 x 192" },
  { value: 256, label: "256 x 256" },
  { value: 320, label: "320 x 320" },
  { value: 512, label: "512 x 512" },
] as const;

type QrGeneratorClientProps = {
  isAuthenticated: boolean;
  locale?: string | null | undefined;
};

async function dataUrlToFile(dataUrl: string, fileName: string, mimeType: string): Promise<File> {
  const response = await fetch(dataUrl);
  const blob = await response.blob();
  return new File([blob], fileName, { type: mimeType });
}

export function QrGeneratorClient({ isAuthenticated, locale }: QrGeneratorClientProps) {
  const [inputValue, setInputValue] = useState(DEFAULT_INPUT);
  const [qrSize, setQrSize] = useState(DEFAULT_QR_SIZE);
  const [qrCorrectionLevel, setQrCorrectionLevel] = useState<QrErrorCorrection>(DEFAULT_QR_ECL);
  const [previewDataUrl, setPreviewDataUrl] = useState<string | null>(null);

  const validation = useMemo(() => validateQrInput(inputValue, locale), [inputValue, locale]);

  useEffect(() => {
    let isCancelled = false;

    async function updatePreview() {
      if (!validation.isValid) {
        setPreviewDataUrl(null);
        return;
      }

      const dataUrl = await QRCode.toDataURL(validation.normalizedValue, {
        width: qrSize,
        margin: 1,
        errorCorrectionLevel: qrCorrectionLevel,
      });

      if (!isCancelled) {
        setPreviewDataUrl(dataUrl);
      }
    }

    void updatePreview();

    return () => {
      isCancelled = true;
    };
  }, [qrCorrectionLevel, qrSize, validation]);

  const canDownload = validation.isValid && previewDataUrl !== null;

  return (
    <section className="mx-auto w-full max-w-4xl px-4 pb-10 sm:px-6 lg:px-8">
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <QrInputPanel
          value={inputValue}
          errorMessage={validation.isValid ? null : validation.errorMessage}
          size={qrSize}
          correctionLevel={qrCorrectionLevel}
          sizeOptions={[...sizeOptions]}
          onValueChange={setInputValue}
          onSizeChange={setQrSize}
          onCorrectionLevelChange={setQrCorrectionLevel}
          locale={locale}
        />
        <QrPreviewPanel previewDataUrl={previewDataUrl} locale={locale} />
      </div>

      <Card className="mt-6">
        <CardContent className="pt-6">
          <QrDownloadActions
            canDownload={canDownload}
            locale={locale}
            onDownload={() => previewDataUrl && downloadQrPng(previewDataUrl)}
          />

          <SaveToHistoryPanel
            toolSlug="qr-generator"
            isAuthenticated={isAuthenticated}
            canSave={canDownload}
            locale={locale}
            getPayload={async () => {
              if (!validation.isValid || !previewDataUrl) {
                return null;
              }

              const file = await dataUrlToFile(
                previewDataUrl,
                "netmetric-qr-code.png",
                "image/png",
              );

              return {
                outputFile: file,
                inputSummaryJson: JSON.stringify({
                  inputLength: validation.normalizedValue.length,
                  size: qrSize,
                  errorCorrectionLevel: qrCorrectionLevel,
                }),
              };
            }}
          />
        </CardContent>
      </Card>
    </section>
  );
}
