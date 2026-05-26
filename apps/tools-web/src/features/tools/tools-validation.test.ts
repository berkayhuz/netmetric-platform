import { describe, expect, it } from "vitest";

import { validateQrInput, QR_MAX_INPUT_LENGTH } from "@/features/tools/qr/qr-validation";
import {
  validateInputFile,
  buildSafeOutputFileName,
} from "@/features/tools/image-converter/image-validation";
import {
  ensureSaveFileConstraints,
  normalizeArtifactFileName,
  getFallbackExtension,
} from "@/features/tools/actions/tool-history-rules";

describe("tools-web validators", () => {
  it("validates QR payload length", () => {
    expect(validateQrInput("ok").isValid).toBe(true);
    const tooLong = validateQrInput("a".repeat(QR_MAX_INPUT_LENGTH + 1));
    expect(tooLong.isValid).toBe(false);
  });

  it("normalizes output names and save constraints", () => {
    expect(buildSafeOutputFileName("Invoice 2026.png", "png-to-jpg")).toBe(
      "invoice-2026-converted.jpg",
    );
    expect(normalizeArtifactFileName("  Report Final.PNG ", "png")).toBe("report-final.png");
    expect(getFallbackExtension("image/jpeg")).toBe("jpg");
    expect(
      ensureSaveFileConstraints({ toolSlug: "qr-generator", mimeType: "image/png", fileSize: 1024 })
        .ok,
    ).toBe(true);
  });

  it("rejects invalid image uploads", () => {
    const invalidType = validateInputFile(
      new File(["a"], "file.gif", { type: "image/gif" }),
      "png-to-jpg",
    );
    expect(invalidType.isValid).toBe(false);
  });
});
