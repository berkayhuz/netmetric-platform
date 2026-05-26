import { resolveLocale, type Locale } from "@netmetric/i18n";

export function mapAccountLanguageToLocale(language: string | null | undefined): Locale {
  if (!language) {
    return "en-US";
  }

  return resolveLocale(language);
}

export function mapAccountThemeToUiTheme(
  theme: string | null | undefined,
): "system" | "light" | "dark" {
  const normalized = (theme ?? "").trim().toLowerCase();
  if (normalized === "dark") {
    return "dark";
  }

  if (normalized === "default") {
    return "system";
  }

  if (normalized === "light") {
    return "light";
  }

  return "system";
}
