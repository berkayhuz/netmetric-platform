import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const headerPath = path.resolve(__dirname, "./public-header.tsx");
const layoutPath = path.resolve(__dirname, "../../../app/layout.tsx");

describe("public header auth-aware navigation", () => {
  it("uses shared session state to switch signed-in and signed-out actions", () => {
    const headerSource = fs.readFileSync(headerPath, "utf8");
    const layoutSource = fs.readFileSync(layoutPath, "utf8");

    expect(layoutSource).toContain("getCurrentPublicSession");
    expect(layoutSource).toContain("<PublicHeader");
    expect(layoutSource).toContain("isAuthenticated: session.isAuthenticated");
    expect(headerSource).toContain("session: PublicHeaderSession");
    expect(headerSource).toContain("copy.signIn");
    expect(headerSource).toContain("copy.account");
    expect(headerSource).toContain("copy.dashboard");
    expect(headerSource).toContain("copy.signOut");
    expect(headerSource).toContain('action="/api/auth/logout"');
    expect(headerSource).toContain("publicEnv.accountUrl");
    expect(headerSource).toContain("publicEnv.crmUrl");
    expect(headerSource).not.toContain("localStorage");
  });
});
