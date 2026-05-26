import { cookies, headers } from "next/headers";
import { cache } from "react";
import { extractLocaleFromAcceptLanguage, UI_LOCALE_COOKIE_NAME } from "@netmetric/i18n";

import { getTranslator, resolveLocale, type Locale } from "./auth-i18n.shared";

export const getRequestLocale = cache(async (): Promise<Locale> => {
  const cookieStore = await cookies();
  const cookieLocale = cookieStore.get(UI_LOCALE_COOKIE_NAME)?.value;

  if (cookieLocale) {
    return resolveLocale(cookieLocale);
  }

  const headerStore = await headers();
  return extractLocaleFromAcceptLanguage(headerStore.get("accept-language"));
});

export { getTranslator };
