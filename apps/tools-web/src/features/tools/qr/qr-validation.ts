import { tTools } from "@/lib/i18n/tools-i18n";

export const QR_MAX_INPUT_LENGTH = 2048;

export type QrValidationResult =
  | { isValid: true; normalizedValue: string }
  | { isValid: false; normalizedValue: string; errorMessage: string };

export function validateQrInput(
  rawValue: string,
  locale?: string | null | undefined,
): QrValidationResult {
  const normalizedValue = rawValue.trim();

  if (normalizedValue.length === 0) {
    return {
      isValid: false,
      normalizedValue,
      errorMessage: tTools("tools.qr.validation.required", locale),
    };
  }

  if (normalizedValue.length > QR_MAX_INPUT_LENGTH) {
    return {
      isValid: false,
      normalizedValue,
      errorMessage: tTools("tools.qr.validation.maxLength", locale, {
        max: QR_MAX_INPUT_LENGTH,
      }),
    };
  }

  return {
    isValid: true,
    normalizedValue,
  };
}
