export const defaultLocale = "en-US";

export type Locale = string;

const localePattern = /^[A-Za-z]{2,3}(?:-[A-Za-z0-9]{2,8})*$/;

function normalizeLocaleInput(value: string): string | null {
  const trimmed = value.trim();
  if (!trimmed || !localePattern.test(trimmed)) {
    return null;
  }

  try {
    return Intl.getCanonicalLocales(trimmed)[0] ?? null;
  } catch {
    return null;
  }
}

export function isLocale(value: string | null | undefined): value is Locale {
  if (!value) {
    return false;
  }

  return normalizeLocaleInput(value) !== null;
}

export function canonicalizeLocale(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  return normalizeLocaleInput(value);
}
