import {
  createTranslator,
  defaultLocale,
  resolveLocale,
  type Locale,
  type MessageKey,
  type Translate,
} from "@netmetric/i18n";

export { defaultLocale, resolveLocale, type Locale, type MessageKey, type Translate };

export function getTranslator(locale: Locale): Translate {
  return createTranslator(locale);
}
