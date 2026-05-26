/// <reference types="node" />

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const tokensPath = path.resolve(__dirname, "../styles/tokens.css");
const providerPath = path.resolve(__dirname, "../components/theme/theme-provider.tsx");
const initScriptPath = path.resolve(__dirname, "../components/theme/theme-init-script.tsx");

describe("dark mode contract", () => {
  it("defines dark token overrides", () => {
    const tokensCss = fs.readFileSync(tokensPath, "utf8");
    expect(tokensCss).toContain(".dark");
    expect(tokensCss).toContain("--background");
    expect(tokensCss).toContain("--foreground");
    expect(tokensCss).toContain("--destructive-foreground");
    expect(tokensCss).toContain("--sidebar");
    expect(tokensCss).toContain("--sidebar-primary: oklch(0.922 0 0);");
  });

  it("uses class-based dark mode toggling in ThemeProvider", () => {
    const provider = fs.readFileSync(providerPath, "utf8");
    expect(provider).toContain('classList.toggle("dark"');
    expect(provider).toContain("colorScheme");
  });

  it("keeps cookie theme ahead of stale localStorage values", () => {
    const provider = fs.readFileSync(providerPath, "utf8");
    const initScript = fs.readFileSync(initScriptPath, "utf8");

    expect(provider).toContain("cookieTheme ??");
    expect(provider).toContain("window.localStorage.setItem(THEME_STORAGE_KEY, initialTheme)");
    expect(initScript).toContain("cookieTheme||");
    expect(initScript).toContain("localStorage.setItem(storageKey,theme)");
  });
});
