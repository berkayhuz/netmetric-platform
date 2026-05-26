import { createRequire } from "node:module";

const require = createRequire(import.meta.url);

function tryLoadScanner(packageName) {
  try {
    const loadedModule = require(packageName);

    if (typeof loadedModule === "function") {
      return loadedModule;
    }

    if (typeof loadedModule.default === "function") {
      return loadedModule.default;
    }

    if (typeof loadedModule.scan === "function") {
      return loadedModule.scan;
    }

    if (typeof loadedModule.scanner === "function") {
      return loadedModule.scanner;
    }

    return null;
  } catch {
    return null;
  }
}

const scanner = tryLoadScanner("@sonar/scan") ?? tryLoadScanner("sonarqube-scanner");

if (typeof scanner !== "function") {
  console.error("SonarScanner for NPM could not be loaded. Install one of these packages first:");
  console.error("  pnpm add -D @sonar/scan -w");
  console.error("or");
  console.error("  pnpm add -D sonarqube-scanner -w");
  process.exit(1);
}

const sonarUrl = process.env.SONAR_HOST_URL || "http://localhost:9000";
const token = process.env.SONAR_TOKEN;

if (!token || token.trim().length === 0) {
  console.error(
    "SONAR_TOKEN environment variable is required. Set it first: $env:SONAR_TOKEN='your-token'",
  );
  process.exit(1);
}

scanner(
  {
    serverUrl: sonarUrl,
    token,
    options: {
      "sonar.projectKey": "netmetric_frontend",
      "sonar.projectName": "NetMetric Frontend",
      "sonar.sourceEncoding": "UTF-8",

      "sonar.sources": "apps,packages/frontend",
      "sonar.tests": "packages/frontend/ui/src/test,packages/frontend/config/src/test",

      "sonar.inclusions": "**/*.ts,**/*.tsx,**/*.js,**/*.mjs,**/*.css",
      "sonar.test.inclusions": "**/*.test.ts,**/*.test.tsx,**/*.spec.ts,**/*.spec.tsx",

      "sonar.exclusions":
        "**/node_modules/**,**/.next/**,**/dist/**,**/build/**,**/coverage/**,**/.turbo/**,**/*.d.ts,**/*.generated.*,**/generated/**,**/vendor/**",

      "sonar.cpd.exclusions": "**/*.test.ts,**/*.test.tsx,**/*.spec.ts,**/*.spec.tsx",

      "sonar.javascript.lcov.reportPaths":
        "packages/frontend/ui/coverage/lcov.info,packages/frontend/config/coverage/lcov.info",

      "sonar.typescript.tsconfigPaths":
        "packages/frontend/ui/tsconfig.json,packages/frontend/config/tsconfig.json,packages/frontend/types/tsconfig.json",
    },
  },
  () => process.exit(),
);
