import "server-only";

import { cookies } from "next/headers";
import {
  resolveDateFormatPreference,
  resolveTimeZonePreference,
  UI_DATE_FORMAT_COOKIE_NAME,
  UI_TIME_ZONE_COOKIE_NAME,
} from "@netmetric/i18n";

import type { CrmDateSettings } from "@/lib/date-time/crm-date-time";

import { getRequestLocale } from "./request-locale";

export async function getRequestDateSettings(): Promise<CrmDateSettings> {
  const [locale, cookieStore] = await Promise.all([getRequestLocale(), cookies()]);

  return {
    locale,
    timeZone: resolveTimeZonePreference(cookieStore.get(UI_TIME_ZONE_COOKIE_NAME)?.value),
    dateFormat: resolveDateFormatPreference(cookieStore.get(UI_DATE_FORMAT_COOKIE_NAME)?.value),
  };
}
