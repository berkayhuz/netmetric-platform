import manifest from "./messages/supported-languages.json";
import { canonicalizeLocale } from "./locales";

export type SupportedLanguage = {
  code: string;
  messageFile: string;
  nativeName: string;
  englishName: string;
};

const supportedLanguages = manifest as readonly SupportedLanguage[];

const supportedLanguageByCode = new Map(
  supportedLanguages.map((item) => [item.code.toLowerCase(), item]),
);

function findByLanguageSubtag(value: string): SupportedLanguage | null {
  const language = value.split("-")[0];
  if (!language) {
    return null;
  }

  for (const item of supportedLanguages) {
    if (item.code.toLowerCase().startsWith(`${language.toLowerCase()}-`)) {
      return item;
    }
  }

  return null;
}

export function getSupportedLanguages(): readonly SupportedLanguage[] {
  return supportedLanguages;
}

export function isSupportedLanguageCode(value: string | null | undefined): boolean {
  const canonical = canonicalizeLocale(value);
  if (!canonical) {
    return false;
  }

  const normalized = canonical.toLowerCase();
  if (supportedLanguageByCode.has(normalized)) {
    return true;
  }

  return findByLanguageSubtag(normalized) !== null;
}

export function resolveSupportedLanguageCode(
  value: string | null | undefined,
  fallback = "en-US",
): string {
  const fallbackCanonical = canonicalizeLocale(fallback) ?? "en-US";
  const canonical = canonicalizeLocale(value);
  if (!canonical) {
    return fallbackCanonical;
  }

  const normalized = canonical.toLowerCase();
  const direct = supportedLanguageByCode.get(normalized);
  if (direct) {
    return direct.code;
  }

  return findByLanguageSubtag(normalized)?.code ?? fallbackCanonical;
}
