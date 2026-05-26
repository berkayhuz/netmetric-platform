import { describe, expect, it } from "vitest";

import {
  createAppBackgroundStyle,
  loadAppBackgroundDataUrls,
  sanitizeAppBackgroundDataUrl,
} from "../components/shell/app-background";

describe("app background assets", () => {
  it("loads the package-level light and dark data URL assets", () => {
    const backgrounds = loadAppBackgroundDataUrls();

    expect(backgrounds.light).toContain("data:image/png;base64,");
    expect(backgrounds.dark).toContain("data:image/jpeg;base64,");
  });

  it("creates theme-aware CSS variables from validated image data URLs", () => {
    const style = createAppBackgroundStyle({
      light: "data:image/png;base64,AAAA",
      dark: "data:image/jpeg;base64,BBBB",
    });

    expect(style["--netmetric-app-background-image-light"]).toBe(
      'url("data:image/png;base64,AAAA")',
    );
    expect(style["--netmetric-app-background-image-dark"]).toBe(
      'url("data:image/jpeg;base64,BBBB")',
    );
  });

  it("rejects non-image or malformed data URL content", () => {
    expect(() => sanitizeAppBackgroundDataUrl("javascript:alert(1)", "light")).toThrow(
      "Invalid light app background image data URL.",
    );
    expect(() => sanitizeAppBackgroundDataUrl("data:text/html;base64,AAAA", "dark")).toThrow(
      "Invalid dark app background image data URL.",
    );
  });
});
