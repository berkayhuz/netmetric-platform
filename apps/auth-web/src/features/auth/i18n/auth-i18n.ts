import { cookies, headers } from "next/headers";
import {
  createTranslator,
  extractLocaleFromAcceptLanguage,
  resolveLocale,
  UI_LOCALE_COOKIE_NAME,
  type Locale,
  type MessageKey,
  type Translate,
} from "@netmetric/i18n";

export async function getRequestLocale(): Promise<Locale> {
  const cookieStore = await cookies();
  const cookieLocale = cookieStore.get(UI_LOCALE_COOKIE_NAME)?.value;

  if (cookieLocale) {
    return resolveLocale(cookieLocale);
  }

  const headerStore = await headers();
  return extractLocaleFromAcceptLanguage(headerStore.get("accept-language"));
}

export function getClientLocale(): Locale {
  if (typeof document === "undefined") {
    return "en";
  }

  return resolveLocale(document.documentElement.lang);
}

export function getTranslator(locale: Locale): Translate {
  return createTranslator(locale);
}

export function tClient(key: MessageKey): string {
  return createTranslator(getClientLocale())(key);
}
