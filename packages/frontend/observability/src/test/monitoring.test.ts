import { describe, expect, it } from "vitest";

import { createMonitoringConfig, redactEvent } from "../index";

describe("monitoring config", () => {
  it("uses noop provider in development and test", () => {
    expect(
      createMonitoringConfig({ app: "auth-web", environment: "development", provider: "console" })
        .provider,
    ).toBe("noop");
    expect(
      createMonitoringConfig({ app: "auth-web", environment: "test", provider: "console" })
        .provider,
    ).toBe("noop");
  });

  it("keeps configured provider in staging and production", () => {
    expect(
      createMonitoringConfig({ app: "auth-web", environment: "staging", provider: "console" })
        .provider,
    ).toBe("console");
    expect(
      createMonitoringConfig({ app: "auth-web", environment: "production", provider: "console" })
        .provider,
    ).toBe("console");
  });
});

describe("redaction", () => {
  it("masks sensitive tags and email addresses", () => {
    const result = redactEvent({
      name: "error",
      message: "failed for john@netmetric.net",
      tags: { endpoint: "/login", email: "john@netmetric.net" },
      extras: { token: "abc", detail: "contact john@netmetric.net" },
    });

    expect(result.message).toContain("[redacted-email]");
    expect(result.tags.email).toBe("[redacted]");
    expect(result.extras?.token).toBe("[redacted]");
    expect(result.extras?.detail).toBe("contact [redacted-email]");
  });
});
