import { afterEach, describe, expect, it, vi } from "vitest";

import robots from "@/app/robots";
import sitemap from "@/app/sitemap";

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("public-web metadata routes", () => {
  it("builds robots with sitemap and host", () => {
    vi.stubEnv("NEXT_PUBLIC_SITE_URL", "https://netmetric.net");
    const value = robots();
    expect(value.sitemap).toContain("/sitemap.xml");
    expect(value.host).toMatch(/^https?:\/\//);
  });

  it("includes homepage and contact in sitemap", () => {
    const value = sitemap();
    const urls = value.map((entry) => entry.url);
    expect(urls).toContainEqual(expect.stringMatching(/\/$/));
    expect(urls.some((url) => url.endsWith("/contact"))).toBe(true);
  });
});
