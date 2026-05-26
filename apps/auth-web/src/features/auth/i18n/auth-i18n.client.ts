"use client";

import {
  defaultLocale,
  getTranslator,
  resolveLocale,
  type Locale,
  type MessageKey,
} from "./auth-i18n.shared";

export function getClientLocale(initialLocale?: Locale): Locale {
  if (initialLocale) {
    return resolveLocale(initialLocale);
  }

  if (typeof document === "undefined") {
    return defaultLocale;
  }

  return resolveLocale(document.documentElement.lang);
}

export function tClient(key: MessageKey, initialLocale?: Locale): string {
  return getTranslator(getClientLocale(initialLocale))(key);
}

export { getTranslator };
