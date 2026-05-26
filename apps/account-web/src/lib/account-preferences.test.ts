import { describe, expect, it } from "vitest";
import { getThemeInitScript } from "@netmetric/ui";
import { getAvailableMessageLocales, getMessages, translate } from "@netmetric/i18n";

import { formatAccountDateTime } from "./account-date";
import { mapAccountLanguageToLocale, mapAccountThemeToUiTheme } from "./account-locale";

describe("account date and ui preference mapping", () => {
  it("formats using selected date format", () => {
    const value = formatAccountDateTime("2026-05-15T10:30:00Z", {
      locale: "en-US",
      timeZone: "UTC",
      dateFormat: "yyyy-MM-dd",
    });

    expect(value).toContain("2026");
    expect(value).toContain("10:30");
  });

  it("falls back safely for invalid date", () => {
    const value = formatAccountDateTime("not-a-date", {
      locale: "en-US",
      timeZone: "UTC",
      dateFormat: "yyyy-MM-dd",
    });

    expect(value).toBe("Not available");
  });

  it("maps language/culture to i18n locale with fallback", () => {
    expect(mapAccountLanguageToLocale("tr-TR")).toBe("tr-TR");
    expect(mapAccountLanguageToLocale("en-US")).toBe("en-US");
    expect(mapAccountLanguageToLocale("zh-CN")).toBe("en-US");
    expect(mapAccountLanguageToLocale("deprecated")).toBe("en-US");
  });

  it("maps theme values to ui themes", () => {
    expect(mapAccountThemeToUiTheme("System")).toBe("system");
    expect(mapAccountThemeToUiTheme("Default")).toBe("system");
    expect(mapAccountThemeToUiTheme("Dark")).toBe("dark");
    expect(mapAccountThemeToUiTheme("Light")).toBe("light");
    expect(mapAccountThemeToUiTheme("Unknown")).toBe("system");
  });

  it("keeps the theme init script aligned with theme preference values", () => {
    const script = getThemeInitScript();

    expect(script).toContain("netmetric-theme");
    expect(script).toContain("cookieTheme||");
    expect(script).toContain("value==='default'");
    expect(script).toContain("matchMedia('(prefers-color-scheme: dark)'");
    expect(script).toContain("classList.toggle('dark'");
  });

  it("loads base messages and falls back to english for unknown locales", () => {
    expect(getMessages("en-US")["locale.name"]).toBe("English");
    expect(getMessages("tr-TR")["locale.name"]).toBe("Turkish");
    expect(translate("locale.name", { locale: "zh-CN" })).toBe("English");
  });

  it("keeps Turkish auth strings as valid UTF-8 text", () => {
    expect(getMessages("tr")["auth.login.cardTitle"]).toBe("NetMetric’e Hoş Geldiniz");
    expect(getMessages("tr")["action.login"]).toBe("Giriş yap");
  });

  it("exposes available message locales from the i18n registry", () => {
    expect(getAvailableMessageLocales()).toContain("en-US");
    expect(getAvailableMessageLocales()).toContain("tr-TR");
  });
});
