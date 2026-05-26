import { existsSync, readdirSync, readFileSync } from "node:fs";
import path from "node:path";

const rootDir = process.cwd();
const violations = [];

const assert = (condition, message) => {
  if (!condition) {
    violations.push(message);
  }
};

const readUtf8 = (relativePath) => readFileSync(path.join(rootDir, relativePath), "utf8");

const readJson = (relativePath) => JSON.parse(readUtf8(relativePath));

const packageDirs = readdirSync(path.join(rootDir, "packages", "frontend"), {
  withFileTypes: true,
})
  .filter((entry) => entry.isDirectory())
  .map((entry) => entry.name);

for (const packageDir of packageDirs) {
  const packageJsonPath = path.join("packages", "frontend", packageDir, "package.json");
  if (!existsSync(path.join(rootDir, packageJsonPath))) {
    continue;
  }

  const pkg = readJson(packageJsonPath);
  assert(
    typeof pkg.name === "string" && pkg.name.startsWith("@netmetric/"),
    `Package naming contract failed for ${packageJsonPath}. Expected @netmetric/* namespace.`,
  );
}

const uiPackage = readJson("packages/frontend/ui/package.json");
assert(Boolean(uiPackage.exports?.["."]), "UI package must expose root export '.'");
assert(Boolean(uiPackage.exports?.["./client"]), "UI package must expose './client' export");
assert(
  Boolean(uiPackage.exports?.["./styles/theme.css"]),
  "UI package must expose './styles/theme.css' export",
);
assert(
  Boolean(uiPackage.exports?.["./styles/tokens.css"]),
  "UI package must expose './styles/tokens.css' export",
);

const configPackage = readJson("packages/frontend/config/package.json");
assert(Boolean(configPackage.exports?.["."]), "Config package must expose root export '.'");
assert(Boolean(configPackage.exports?.["./env"]), "Config package must expose './env' export");

const envContract = readUtf8("packages/frontend/config/src/env.ts");
for (const key of ["NODE_ENV", "APP_ENV", "NEXT_PUBLIC_APP_NAME", "NEXT_PUBLIC_API_BASE_URL"]) {
  assert(envContract.includes(key), `Env contract is missing required key definition: ${key}`);
}

const tokensCss = readUtf8("packages/frontend/ui/src/styles/tokens.css");
const themeCss = readUtf8("packages/frontend/ui/src/styles/theme.css");
assert(tokensCss.includes(":root"), "Token contract must define :root tokens.");
assert(tokensCss.includes(".dark"), "Dark mode token contract must define .dark tokens.");
assert(themeCss.includes("--color-background"), "Theme contract must expose --color-background.");
assert(themeCss.includes("--color-focus-ring"), "Theme contract must expose --color-focus-ring.");
assert(themeCss.includes("--animate-base"), "Theme contract must expose motion tokens.");

const packageFiles = ["packages/frontend/ui/src/index.ts", "packages/frontend/ui/src/client.ts"];
for (const relativePath of packageFiles) {
  const file = readUtf8(relativePath);
  assert(
    !file.includes("@netmetric/ui/src/"),
    `Public API contract violation in ${relativePath}: internal src import exposed.`,
  );
}

const scanTsFiles = (startDir) => {
  const pending = [path.join(rootDir, startDir)];
  while (pending.length > 0) {
    const current = pending.pop();
    if (!current) {
      continue;
    }
    for (const entry of readdirSync(current, { withFileTypes: true })) {
      const absolutePath = path.join(current, entry.name);
      if (entry.isDirectory()) {
        if (entry.name === "node_modules" || entry.name === ".next" || entry.name === "dist") {
          continue;
        }
        pending.push(absolutePath);
        continue;
      }

      if (!/\.(ts|tsx|js|mjs)$/.test(entry.name)) {
        continue;
      }

      const relativePath = path.relative(rootDir, absolutePath).replaceAll("\\", "/");
      const content = readFileSync(absolutePath, "utf8");

      if (
        relativePath.startsWith("packages/frontend/") &&
        /(from|import)\s+["'][^"']*apps\//.test(content)
      ) {
        violations.push(`Architecture contract violation: ${relativePath} imports from apps/*`);
      }

      if (/@netmetric\/ui\/src\//.test(content)) {
        violations.push(
          `Architecture contract violation: ${relativePath} imports private @netmetric/ui/src path.`,
        );
      }
    }
  }
};

scanTsFiles("packages/frontend");

if (violations.length > 0) {
  console.error("Contract validation failed:");
  for (const violation of violations) {
    console.error(`- ${violation}`);
  }
  process.exit(1);
}

console.log("Contract validation passed.");
