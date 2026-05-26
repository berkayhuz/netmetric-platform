import { describe, expect, it } from "vitest";

import { parseFrontendEnv } from "../env";

describe("frontend env contract", () => {
  it("accepts valid required fields", () => {
    const env = parseFrontendEnv({
      NODE_ENV: "development",
      APP_ENV: "development",
      NEXT_PUBLIC_APP_NAME: "NetMetric",
      NEXT_PUBLIC_API_BASE_URL: "https://api.netmetric.test",
    });

    expect(env.APP_ENV).toBe("development");
    expect(env.NEXT_PUBLIC_API_BASE_URL).toBe("https://api.netmetric.test");
  });

  it("accepts valid optional sentry urls", () => {
    const env = parseFrontendEnv({
      NODE_ENV: "production",
      APP_ENV: "production",
      NEXT_PUBLIC_APP_NAME: "NetMetric",
      NEXT_PUBLIC_API_BASE_URL: "https://api.netmetric.test",
      SENTRY_DSN: "https://public@sentry.io/1",
      NEXT_PUBLIC_SENTRY_DSN: "https://public@sentry.io/2",
    });

    expect(env.SENTRY_DSN).toBe("https://public@sentry.io/1");
    expect(env.NEXT_PUBLIC_SENTRY_DSN).toBe("https://public@sentry.io/2");
  });

  it("accepts missing optional sentry urls", () => {
    const env = parseFrontendEnv({
      NODE_ENV: "test",
      APP_ENV: "test",
      NEXT_PUBLIC_APP_NAME: "NetMetric",
      NEXT_PUBLIC_API_BASE_URL: "https://api.netmetric.test",
    });

    expect(env.SENTRY_DSN).toBeUndefined();
    expect(env.NEXT_PUBLIC_SENTRY_DSN).toBeUndefined();
  });

  it("rejects invalid app environment values", () => {
    expect(() =>
      parseFrontendEnv({
        NODE_ENV: "development",
        APP_ENV: "sandbox",
        NEXT_PUBLIC_APP_NAME: "NetMetric",
        NEXT_PUBLIC_API_BASE_URL: "https://api.netmetric.test",
      }),
    ).toThrow();
  });

  it("rejects invalid public API URL", () => {
    expect(() =>
      parseFrontendEnv({
        NODE_ENV: "development",
        APP_ENV: "development",
        NEXT_PUBLIC_APP_NAME: "NetMetric",
        NEXT_PUBLIC_API_BASE_URL: "not-a-url",
      }),
    ).toThrow();
  });

  it("rejects invalid sentry URL values", () => {
    expect(() =>
      parseFrontendEnv({
        NODE_ENV: "development",
        APP_ENV: "development",
        NEXT_PUBLIC_APP_NAME: "NetMetric",
        NEXT_PUBLIC_API_BASE_URL: "https://api.netmetric.test",
        SENTRY_DSN: "not-a-url",
      }),
    ).toThrow();
  });
});
