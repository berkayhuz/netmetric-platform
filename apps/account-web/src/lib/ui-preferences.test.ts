import { describe, expect, it } from "vitest";
import { getPreferenceCookieOptions, resolveUiPreferences } from "@netmetric/i18n";

import { resolvePreferenceCookiesFromPayload } from "./ui-preference-cookies";

describe("shared ui preference resolver", () => {
  it("reads theme and locale values from shared cookies", () => {
    const resolved = resolveUiPreferences({ theme: "dark", locale: "tr-TR" });
    expect(resolved.theme).toBe("dark");
    expect(resolved.locale).toBe("tr-TR");
    expect(resolved.timeZone).toBe("UTC");
    expect(resolved.dateFormat).toBe("yyyy-MM-dd");
  });

  it("falls back safely for invalid cookie values", () => {
    const resolved = resolveUiPreferences({
      theme: "invalid",
      locale: "___",
      timeZone: "Invalid/Zone",
      dateFormat: "invalid",
    });
    expect(resolved.theme).toBe("system");
    expect(resolved.locale).toBe("en-US");
    expect(resolved.timeZone).toBe("UTC");
    expect(resolved.dateFormat).toBe("yyyy-MM-dd");
  });

  it("normalizes legacy Default theme values to system", () => {
    const resolved = resolveUiPreferences({ theme: "Default", locale: "en-US" });
    expect(resolved.theme).toBe("system");
  });

  it("normalizes payload values before writing shared cookies", () => {
    const cookieValues = resolvePreferenceCookiesFromPayload({
      theme: "Dark",
      language: "tr-tr",
      timeZone: "Europe/Istanbul",
      dateFormat: "dd.MM.yyyy",
    });

    expect(cookieValues.theme).toBe("dark");
    expect(cookieValues.locale).toBe("tr-TR");
    expect(cookieValues.timeZone).toBe("Europe/Istanbul");
    expect(cookieValues.dateFormat).toBe("dd.MM.yyyy");
  });

  it("uses host-only cookies on localhost and shared domain in production", () => {
    const local = getPreferenceCookieOptions({
      appOrigin: "http://localhost:7004",
      cookieDomain: ".netmetric.net",
    });
    expect(local.domain).toBeUndefined();
    expect(local.secure).toBe(false);

    const production = getPreferenceCookieOptions({
      appOrigin: "https://account.netmetric.net",
      cookieDomain: ".netmetric.net",
    });
    expect(production.domain).toBe(".netmetric.net");
    expect(production.secure).toBe(true);
  });
});
