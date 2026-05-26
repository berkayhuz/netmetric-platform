import {
  resolveDateFormatPreference,
  resolveLocale,
  resolveThemePreference,
  resolveTimeZonePreference,
} from "@netmetric/i18n";

export function resolvePreferenceCookiesFromPayload(payload: {
  theme: string;
  language: string;
  timeZone?: string | null;
  dateFormat?: string | null;
}): {
  theme: "light" | "dark" | "system";
  locale: string;
  timeZone: string;
  dateFormat: string;
} {
  return {
    theme: resolveThemePreference(payload.theme),
    locale: resolveLocale(payload.language),
    timeZone: resolveTimeZonePreference(payload.timeZone),
    dateFormat: resolveDateFormatPreference(payload.dateFormat),
  };
}
