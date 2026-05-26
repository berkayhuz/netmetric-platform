import { describe, expect, it } from "vitest";
import {
  getSupportedLanguages,
  isSupportedLanguageCode,
  resolveSupportedLanguageCode,
} from "@netmetric/i18n";

import { getSupportedLanguageOptions } from "./supported-language-options";

describe("supported language options", () => {
  it("exposes only manifest-backed languages", () => {
    const options = getSupportedLanguageOptions();
    expect(options.map((item) => item.value)).toEqual(
      getSupportedLanguages().map((item) => item.code),
    );
    expect(options.map((item) => item.value)).not.toContain("zh-CN");
  });

  it("normalizes language-only values to supported locales", () => {
    expect(resolveSupportedLanguageCode("tr")).toBe("tr-TR");
    expect(isSupportedLanguageCode("tr-TR")).toBe(true);
    expect(isSupportedLanguageCode("fr-FR")).toBe(false);
  });
});
