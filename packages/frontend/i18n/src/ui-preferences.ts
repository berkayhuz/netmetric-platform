import { defaultLocale, type Locale } from "./locales";
import { isSupportedLanguageCode } from "./supported-languages";
import { resolveLocale } from "./translate";

export type UiThemePreference = "light" | "dark" | "system";

export const UI_THEME_COOKIE_NAME = "netmetric-theme";
export const UI_LOCALE_COOKIE_NAME = "netmetric-locale";
export const UI_TIME_ZONE_COOKIE_NAME = "netmetric-time-zone";
export const UI_DATE_FORMAT_COOKIE_NAME = "netmetric-date-format";
export const UI_POST_LOGIN_REDIRECT_COOKIE_NAME = "netmetric-post-login-redirect-url";

const ONE_YEAR_SECONDS = 60 * 60 * 24 * 365;

type SameSite = "lax" | "strict" | "none";

export type UiPreferenceCookieOptions = {
  path: string;
  sameSite: SameSite;
  secure: boolean;
  maxAge: number;
  domain?: string;
};

export function resolveThemePreference(value: string | null | undefined): UiThemePreference {
  const normalized = (value ?? "").trim().toLowerCase();
  if (normalized === "light" || normalized === "dark" || normalized === "system") {
    return normalized;
  }

  if (normalized === "default") {
    return "system";
  }

  return "system";
}

export function resolveLocalePreference(value: string | null | undefined): Locale {
  return resolveLocale(value);
}

export function resolveTimeZonePreference(value: string | null | undefined): string {
  const normalized = value?.trim();
  if (!normalized) {
    return "UTC";
  }

  try {
    new Intl.DateTimeFormat("en", { timeZone: normalized }).format(new Date(0));
    return normalized;
  } catch {
    return "UTC";
  }
}

export function resolveDateFormatPreference(value: string | null | undefined): string {
  const normalized = value?.trim();
  switch (normalized) {
    case "yyyy-MM-dd":
    case "dd/MM/yyyy":
    case "MM/dd/yyyy":
    case "dd.MM.yyyy":
    case "d MMM yyyy":
      return normalized;
    default:
      return "yyyy-MM-dd";
  }
}

export function resolveUiPreferences(options?: {
  theme?: string | null | undefined;
  locale?: string | null | undefined;
  timeZone?: string | null | undefined;
  dateFormat?: string | null | undefined;
}): { theme: UiThemePreference; locale: Locale; timeZone: string; dateFormat: string } {
  return {
    theme: resolveThemePreference(options?.theme),
    locale: resolveLocalePreference(options?.locale),
    timeZone: resolveTimeZonePreference(options?.timeZone),
    dateFormat: resolveDateFormatPreference(options?.dateFormat),
  };
}

export function extractLocaleFromAcceptLanguage(value: string | null): Locale {
  if (!value) {
    return defaultLocale;
  }

  const parts = value.split(",");
  for (const part of parts) {
    const candidate = part.split(";")[0]?.trim();
    if (!candidate) {
      continue;
    }

    if (isSupportedLanguageCode(candidate)) {
      return resolveLocale(candidate);
    }
  }

  return defaultLocale;
}

function isLocalHostname(hostname: string): boolean {
  const normalized = hostname.trim().toLowerCase();
  return normalized === "localhost" || normalized === "127.0.0.1" || normalized === "::1";
}

function normalizeCookieDomain(value: string | undefined): string | undefined {
  const normalized = value?.trim();
  if (!normalized) {
    return undefined;
  }

  const lowered = normalized.toLowerCase();
  if (lowered === "localhost" || lowered === "127.0.0.1" || lowered === "::1") {
    return undefined;
  }

  return normalized;
}

export function getPreferenceCookieOptions(config: {
  appOrigin: string;
  cookieDomain?: string | undefined;
  maxAge?: number | undefined;
}): UiPreferenceCookieOptions {
  const hostname = new URL(config.appOrigin).hostname;
  const isLocal = isLocalHostname(hostname);
  const domain = isLocal ? undefined : normalizeCookieDomain(config.cookieDomain);

  return {
    path: "/",
    sameSite: "lax",
    secure: !isLocal,
    maxAge: config.maxAge ?? ONE_YEAR_SECONDS,
    ...(domain ? { domain } : {}),
  };
}
