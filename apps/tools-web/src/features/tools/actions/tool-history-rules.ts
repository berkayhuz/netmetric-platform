import { tTools } from "@/lib/i18n/tools-i18n";

export const saveAllowedSlugs = new Set(["qr-generator", "png-to-jpg", "jpg-to-png"]);

const saveAllowedMimeTypesBySlug: Record<string, ReadonlySet<string>> = {
  "qr-generator": new Set(["image/png"]),
  "png-to-jpg": new Set(["image/jpeg"]),
  "jpg-to-png": new Set(["image/png"]),
};

export const AUTHENTICATED_SAVE_MAX_BYTES = 10 * 1024 * 1024;

function sanitizeBaseName(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9._-]/g, "-")
    .replace(/-+/g, "-")
    .replace(/^[-_.]+|[-_.]+$/g, "");
}

export function normalizeArtifactFileName(fileName: string, fallbackExtension: string): string {
  const dotIndex = fileName.lastIndexOf(".");
  const baseName = dotIndex > 0 ? fileName.slice(0, dotIndex) : fileName;
  const normalizedBase = sanitizeBaseName(baseName);
  const safeBase = normalizedBase.length > 0 ? normalizedBase : "netmetric-output";
  return `${safeBase}.${fallbackExtension}`;
}

export function getFallbackExtension(mimeType: string): string {
  if (mimeType === "image/png") {
    return "png";
  }

  if (mimeType === "image/jpeg") {
    return "jpg";
  }

  return "bin";
}

export function ensureSaveFileConstraints(params: {
  toolSlug: string;
  mimeType: string;
  fileSize: number;
  locale?: string | null | undefined;
}): { ok: true } | { ok: false; message: string } {
  if (!saveAllowedSlugs.has(params.toolSlug)) {
    return { ok: false, message: tTools("tools.history.errors.unsupportedTool", params.locale) };
  }

  if (params.fileSize <= 0) {
    return { ok: false, message: tTools("tools.history.errors.emptyOutput", params.locale) };
  }

  if (params.fileSize > AUTHENTICATED_SAVE_MAX_BYTES) {
    return { ok: false, message: tTools("tools.history.errors.saveLimit", params.locale) };
  }

  const allowedMimeTypes = saveAllowedMimeTypesBySlug[params.toolSlug];
  if (!allowedMimeTypes?.has(params.mimeType)) {
    return { ok: false, message: tTools("tools.history.errors.unsupportedOutput", params.locale) };
  }

  if (params.mimeType === "image/svg+xml") {
    return { ok: false, message: tTools("tools.history.errors.svgUnsupported", params.locale) };
  }

  return { ok: true };
}
