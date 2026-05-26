import { describe, expect, it } from "vitest";

import { resolveLanguageSelectState } from "./language-select";

const baseOptions = [
  { value: "en-US", label: "English (en-US)" },
  { value: "tr-TR", label: "Turkish (tr-TR)" },
];

describe("resolveLanguageSelectState", () => {
  it("selects exact locale when available", () => {
    const state = resolveLanguageSelectState("tr-TR", baseOptions);
    expect(state.selectedValue).toBe("tr-TR");
  });

  it("maps language-only locale to a supported regional locale", () => {
    const state = resolveLanguageSelectState("tr", baseOptions);
    expect(state.selectedValue).toBe("tr-TR");
  });

  it("falls back to the first supported option when saved locale is missing", () => {
    const state = resolveLanguageSelectState("zh-CN", baseOptions);
    expect(state.selectedValue).toBe("en-US");
  });

  it("falls back to the first supported option for invalid values", () => {
    const state = resolveLanguageSelectState("not-a-locale", baseOptions);
    expect(state.selectedValue).toBe("en-US");
  });
});
