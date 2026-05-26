import { tTools } from "@/lib/i18n/tools-i18n";

export const GUEST_MAX_FILE_BYTES = 5 * 1024 * 1024;
export const MAX_IMAGE_PIXELS = 40_000_000;

export type ImageConverterMode = "png-to-jpg" | "jpg-to-png";

export type ImageValidationResult =
  | {
      isValid: true;
      mimeType: string;
      sanitizedFileName: string;
    }
  | {
      isValid: false;
      errorMessage: string;
    };

const ACCEPTED_MIME_BY_MODE: Record<ImageConverterMode, ReadonlySet<string>> = {
  "png-to-jpg": new Set(["image/png"]),
  "jpg-to-png": new Set(["image/jpeg"]),
};

function getSafeDefaultName(mode: ImageConverterMode): string {
  return mode === "png-to-jpg" ? "netmetric-converted.jpg" : "netmetric-converted.png";
}

function normalizeBaseName(name: string): string {
  const value = name.trim().toLowerCase();
  const safe = value
    .replace(/[^a-z0-9._-]/g, "-")
    .replace(/-+/g, "-")
    .replace(/^[-_.]+|[-_.]+$/g, "");
  return safe.length > 0 ? safe : "netmetric-converted";
}

export function buildSafeOutputFileName(originalName: string, mode: ImageConverterMode): string {
  const outputExtension = mode === "png-to-jpg" ? ".jpg" : ".png";
  const defaultName = getSafeDefaultName(mode);

  const dotIndex = originalName.lastIndexOf(".");
  const baseName = dotIndex > 0 ? originalName.slice(0, dotIndex) : originalName;
  const normalized = normalizeBaseName(baseName);

  if (normalized.length === 0) {
    return defaultName;
  }

  return `${normalized}-converted${outputExtension}`;
}

export function validateInputFile(
  file: File | null,
  mode: ImageConverterMode,
  locale?: string | null | undefined,
): ImageValidationResult {
  if (!file) {
    return {
      isValid: false,
      errorMessage: tTools("tools.image.validation.required", locale),
    };
  }

  const accepted = ACCEPTED_MIME_BY_MODE[mode];

  if (!accepted.has(file.type)) {
    return {
      isValid: false,
      errorMessage:
        mode === "png-to-jpg"
          ? tTools("tools.image.validation.pngOnly", locale)
          : tTools("tools.image.validation.jpgOnly", locale),
    };
  }

  if (file.size > GUEST_MAX_FILE_BYTES) {
    return {
      isValid: false,
      errorMessage: tTools("tools.image.validation.maxSize", locale),
    };
  }

  return {
    isValid: true,
    mimeType: file.type,
    sanitizedFileName: buildSafeOutputFileName(file.name, mode),
  };
}
