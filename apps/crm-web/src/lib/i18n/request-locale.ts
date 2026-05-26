import "server-only";

import { cookies, headers } from "next/headers";
import {
  extractLocaleFromAcceptLanguage,
  resolveLocale,
  UI_LOCALE_COOKIE_NAME,
  type Locale,
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
