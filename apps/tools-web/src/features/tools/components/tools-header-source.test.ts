import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const headerPath = path.resolve(__dirname, "./tools-header.tsx");
const layoutPath = path.resolve(__dirname, "../../../app/layout.tsx");
const authPath = path.resolve(__dirname, "../../../lib/tools-auth/tools-auth-headers.ts");

describe("tools header auth-aware navigation", () => {
  it("hydrates shared signed-in state for header and protected tools", () => {
    const headerSource = fs.readFileSync(headerPath, "utf8");
    const layoutSource = fs.readFileSync(layoutPath, "utf8");
    const authSource = fs.readFileSync(authPath, "utf8");

    expect(layoutSource).toContain("getCurrentToolsSession");
    expect(layoutSource).toContain("isAuthenticated={session.isAuthenticated}");
    expect(headerSource).toContain("isAuthenticated: boolean");
    expect(headerSource).toContain('tTools("tools.actions.signIn", locale)');
    expect(headerSource).toContain('tTools("tools.actions.signOut", locale)');
    expect(headerSource).toContain('action="/api/auth/logout"');
    expect(authSource).toContain("resolveCurrentSession");
    expect(authSource).toContain("getAccessTokenFromCookieHeader");
    expect(authSource).not.toContain("localStorage");
  });
});
