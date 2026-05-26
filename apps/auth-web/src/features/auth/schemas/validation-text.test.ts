import { describe, expect, it } from "vitest";

import { getValidationText } from "./validation-text";

describe("validation text contract", () => {
  it("resolves shared required/invalid-email messages in EN and TR", () => {
    const en = getValidationText("en");
    const tr = getValidationText("tr");

    expect(en.emailRequired).toBe("This field is required.");
    expect(en.emailInvalid).toBe("Enter a valid email address.");
    expect(tr.emailRequired).toBe("Bu alan zorunludur.");
    expect(tr.emailInvalid.toLowerCase()).toContain("e-posta");
  });
});
