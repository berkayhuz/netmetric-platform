import { describe, expect, it, vi } from "vitest";

import { normalizeMediaUrl, shouldUseUnoptimizedAvatar } from "./avatar-media";

describe("avatar-media", () => {
  it("keeps absolute localhost upload url", () => {
    expect(
      normalizeMediaUrl(
        "http://localhost:5301/uploads/netmetric/media/u1/avatar/2026/05/a1/original.jpg",
      ),
    ).toBe("http://localhost:5301/uploads/netmetric/media/u1/avatar/2026/05/a1/original.jpg");
  });

  it("keeps absolute CDN url", () => {
    expect(
      normalizeMediaUrl("https://cdn.netmetric.net/netmetric/media/u1/avatar/original.jpg"),
    ).toBe("https://cdn.netmetric.net/netmetric/media/u1/avatar/original.jpg");
  });

  it("returns null for missing avatar", () => {
    expect(normalizeMediaUrl(null)).toBeNull();
    expect(normalizeMediaUrl("   ")).toBeNull();
  });

  it("resolves relative uploads against configured api origin", () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "http://localhost:5301");
    expect(normalizeMediaUrl("/uploads/netmetric/media/u1/avatar/original.jpg")).toBe(
      "http://localhost:5301/uploads/netmetric/media/u1/avatar/original.jpg",
    );
    vi.unstubAllEnvs();
  });

  it("uses unoptimized only for development localhost uploads", () => {
    vi.stubEnv("NODE_ENV", "development");
    expect(
      shouldUseUnoptimizedAvatar(
        "http://localhost:5301/uploads/netmetric/media/u1/avatar/original.jpg",
      ),
    ).toBe(true);
    expect(
      shouldUseUnoptimizedAvatar(
        "https://cdn.netmetric.net/netmetric/media/u1/avatar/original.jpg",
      ),
    ).toBe(false);
    vi.unstubAllEnvs();
  });
});
