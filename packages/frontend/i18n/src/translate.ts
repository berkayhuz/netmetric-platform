import { canonicalizeLocale, defaultLocale, isLocale, type Locale } from "./locales";
import { getFallbackMessages, getMessages, type MessageKey } from "./messages";
import { resolveSupportedLanguageCode } from "./supported-languages";

export type TranslateParams = Record<string, string | number>;

export type Translate = (key: MessageKey, params?: TranslateParams) => string;

function format(template: string, params?: TranslateParams): string {
  if (!params) {
    return template;
  }

  return template.replace(/\{(\w+)\}/g, (match, token: string) => {
    const value = params[token];
    return value === undefined ? match : String(value);
  });
}

export function resolveLocale(value: string | null | undefined): Locale {
  const canonical = canonicalizeLocale(value);
  if (!canonical || !isLocale(canonical)) {
    return defaultLocale;
  }

  return resolveSupportedLanguageCode(canonical, defaultLocale);
}

export function createTranslator(locale: Locale): Translate {
  const dict = getMessages(locale);
  const fallbackDict = getFallbackMessages();

  return (key, params) => {
    const value = dict[key] ?? fallbackDict[key] ?? key;
    return format(value, params);
  };
}

export function translate(
  key: MessageKey,
  options?: {
    locale?: string | null | undefined;
    params?: TranslateParams;
  },
): string {
  const locale = resolveLocale(options?.locale);
  return createTranslator(locale)(key, options?.params);
}
