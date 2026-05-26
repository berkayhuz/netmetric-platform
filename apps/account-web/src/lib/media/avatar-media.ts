const LOCAL_MEDIA_ORIGIN_FALLBACK = "http://localhost:5301";

function parseUrl(value: string): URL | null {
  try {
    return new URL(value);
  } catch {
    return null;
  }
}

function resolveLocalMediaOrigin(): string {
  const configuredOrigin =
    process.env.NEXT_PUBLIC_ACCOUNT_MEDIA_ORIGIN ??
    process.env.ACCOUNT_MEDIA_ORIGIN ??
    process.env.NEXT_PUBLIC_API_BASE_URL ??
    process.env.ACCOUNT_API_BASE_URL;

  const parsed = configuredOrigin ? parseUrl(configuredOrigin) : null;
  if (!parsed) {
    return LOCAL_MEDIA_ORIGIN_FALLBACK;
  }

  return parsed.origin;
}

export function normalizeMediaUrl(value: string | null | undefined): string | null {
  const raw = value?.trim();
  if (!raw) {
    return null;
  }

  if (raw.startsWith("http://") || raw.startsWith("https://")) {
    return raw;
  }

  if (raw.startsWith("/")) {
    return `${resolveLocalMediaOrigin()}${raw}`;
  }

  return null;
}

export function shouldUseUnoptimizedAvatar(src: string): boolean {
  if (process.env.NODE_ENV !== "development") {
    return false;
  }

  const parsed = parseUrl(src);
  if (!parsed) {
    return false;
  }

  const isLocalHost = parsed.hostname === "localhost" || parsed.hostname === "127.0.0.1";
  return parsed.protocol === "http:" && isLocalHost && parsed.pathname.startsWith("/uploads/");
}
