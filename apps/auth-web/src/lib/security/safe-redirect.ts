const fallbackPath = "/";

export function getSafeRedirectPath(value: string | null | undefined): string {
  if (!value) {
    return fallbackPath;
  }

  if (!value.startsWith("/")) {
    return fallbackPath;
  }

  if (value.startsWith("//")) {
    return fallbackPath;
  }

  try {
    const url = new URL(value, "http://localhost");

    if (url.origin !== "http://localhost") {
      return fallbackPath;
    }

    return `${url.pathname}${url.search}${url.hash}`;
  } catch {
    return fallbackPath;
  }
}
