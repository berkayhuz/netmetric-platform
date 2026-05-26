/// <reference types="node" />

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const stylesDir = path.resolve(__dirname, "../styles");

function readStyleFile(name: string) {
  return fs.readFileSync(path.join(stylesDir, name), "utf-8");
}

describe("token/css smoke", () => {
  it("tokens.css contains critical variables", () => {
    const tokensCss = readStyleFile("tokens.css");

    expect(tokensCss).toContain("--background");
    expect(tokensCss).toContain("--foreground");
    expect(tokensCss).toContain("--ring");
    expect(tokensCss).toContain("--destructive-foreground");
    expect(tokensCss).toContain("--sidebar-ring");
    expect(tokensCss).toContain("--chart-1");
    expect(tokensCss).toContain("--chart-5");
    expect(tokensCss).toContain(".dark");
  });

  it("theme.css exposes semantic aliases", () => {
    const themeCss = readStyleFile("theme.css");

    expect(themeCss).toContain("--color-background");
    expect(themeCss).toContain("--color-foreground");
    expect(themeCss).toContain("--color-ring");
    expect(themeCss).toContain("--color-destructive-foreground");
    expect(themeCss).toContain("--color-sidebar-ring");
    expect(themeCss).toContain("--radius-lg");
    expect(themeCss).toContain("--radius-2xl");
  });
});
