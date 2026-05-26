import { Card, CardContent, CardDescription, CardHeader, CardTitle, Textarea } from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import { tTools } from "@/lib/i18n/tools-i18n";

import { QR_MAX_INPUT_LENGTH } from "./qr-validation";

type QrSizeOption = {
  label: string;
  value: number;
};

type QrErrorCorrection = "L" | "M" | "Q" | "H";

type QrInputPanelProps = {
  value: string;
  errorMessage: string | null;
  size: number;
  correctionLevel: QrErrorCorrection;
  sizeOptions: QrSizeOption[];
  onValueChange: (value: string) => void;
  onSizeChange: (value: number) => void;
  onCorrectionLevelChange: (value: QrErrorCorrection) => void;
  locale?: string | null | undefined;
};

export function QrInputPanel({
  value,
  errorMessage,
  size,
  correctionLevel,
  sizeOptions,
  onValueChange,
  onSizeChange,
  onCorrectionLevelChange,
  locale,
}: QrInputPanelProps) {
  const describedBy = errorMessage ? "qr-input-help qr-input-error" : "qr-input-help";

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tTools("tools.qr.input.title", locale)}</CardTitle>
        <CardDescription>{tTools("tools.qr.input.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <label htmlFor="qr-input" className="text-sm font-medium">
            {tTools("tools.qr.input.contentLabel", locale)}
          </label>
          <Textarea
            id="qr-input"
            name="qr-input"
            value={value}
            rows={6}
            maxLength={QR_MAX_INPUT_LENGTH + 50}
            onChange={(event) => onValueChange(event.target.value)}
            aria-invalid={Boolean(errorMessage)}
            aria-describedby={describedBy}
            placeholder="https://example.com"
          />
          <p id="qr-input-help" className="text-xs text-muted-foreground">
            {tTools("tools.qr.input.help", locale, { max: QR_MAX_INPUT_LENGTH })}
          </p>
          {errorMessage ? (
            <p id="qr-input-error" className="text-sm text-destructive" role="alert">
              {errorMessage}
            </p>
          ) : null}
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <label htmlFor="qr-size" className="text-sm font-medium">
              {tTools("tools.qr.input.sizeLabel", locale)}
            </label>
            <Select value={String(size)} onValueChange={(value) => onSizeChange(Number(value))}>
              <SelectTrigger id="qr-size">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {sizeOptions.map((option) => (
                  <SelectItem key={option.value} value={String(option.value)}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <label htmlFor="qr-ecl" className="text-sm font-medium">
              {tTools("tools.qr.input.errorCorrectionLabel", locale)}
            </label>
            <Select
              value={correctionLevel}
              onValueChange={(value) => onCorrectionLevelChange(value as QrErrorCorrection)}
            >
              <SelectTrigger id="qr-ecl">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="L">{tTools("tools.qr.errorCorrection.low", locale)}</SelectItem>
                <SelectItem value="M">
                  {tTools("tools.qr.errorCorrection.medium", locale)}
                </SelectItem>
                <SelectItem value="Q">
                  {tTools("tools.qr.errorCorrection.quartile", locale)}
                </SelectItem>
                <SelectItem value="H">{tTools("tools.qr.errorCorrection.high", locale)}</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
