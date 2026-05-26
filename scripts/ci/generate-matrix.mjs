import { writeFileSync } from "node:fs";

const outputsPath = process.env.GITHUB_OUTPUT;

if (!outputsPath) {
  console.error("GITHUB_OUTPUT is required.");
  process.exit(1);
}

const toBool = (value) => value === "true";

const flags = {
  docs: toBool(process.env.FILTER_DOCS),
  ci: toBool(process.env.FILTER_CI),
  repo: toBool(process.env.FILTER_REPO),
  pkgUi: toBool(process.env.FILTER_PKG_UI),
  pkgConfig: toBool(process.env.FILTER_PKG_CONFIG),
  pkgTypes: toBool(process.env.FILTER_PKG_TYPES),
  pkgLint: toBool(process.env.FILTER_PKG_LINT),
  pkgTsconfig: toBool(process.env.FILTER_PKG_TSCONFIG),
  pkgTailwind: toBool(process.env.FILTER_PKG_TAILWIND),
  appAccount: toBool(process.env.FILTER_APP_ACCOUNT),
  appAdmin: toBool(process.env.FILTER_APP_ADMIN),
  appAuth: toBool(process.env.FILTER_APP_AUTH),
  appCrm: toBool(process.env.FILTER_APP_CRM),
  appMobile: toBool(process.env.FILTER_APP_MOBILE),
  appPublic: toBool(process.env.FILTER_APP_PUBLIC),
  appTools: toBool(process.env.FILTER_APP_TOOLS),
  dotnet: toBool(process.env.FILTER_DOTNET),
  native: toBool(process.env.FILTER_NATIVE),
  deploy: toBool(process.env.FILTER_DEPLOY),
  tests: toBool(process.env.FILTER_TESTS),
  tools: toBool(process.env.FILTER_TOOLS),
};

const allPackages = [
  { name: "@netmetric/ui", path: "packages/frontend/ui" },
  { name: "@netmetric/config", path: "packages/frontend/config" },
  { name: "@netmetric/types", path: "packages/frontend/types" },
  { name: "@netmetric/eslint-config", path: "packages/frontend/eslint-config" },
  { name: "@netmetric/tsconfig", path: "packages/frontend/tsconfig" },
  { name: "@netmetric/tailwind-config", path: "packages/frontend/tailwind-config" },
];

const allApps = [
  { name: "account-web", path: "apps/account-web" },
  { name: "admin-web", path: "apps/admin-web" },
  { name: "auth-web", path: "apps/auth-web" },
  { name: "crm-web", path: "apps/crm-web" },
  { name: "mobile-app", path: "apps/mobile-app" },
  { name: "public-web", path: "apps/public-web" },
  { name: "tools-web", path: "apps/tools-web" },
];

const packageSet = new Map();
const appSet = new Map();

const includePackage = (name) => {
  const found = allPackages.find((pkg) => pkg.name === name);
  if (found) {
    packageSet.set(found.name, found);
  }
};

const includeApp = (name) => {
  const found = allApps.find((app) => app.name === name);
  if (found) {
    appSet.set(found.name, found);
  }
};

const infraTouched = flags.ci || flags.repo;

if (infraTouched) {
  for (const pkg of allPackages) {
    packageSet.set(pkg.name, pkg);
  }
  for (const app of allApps) {
    appSet.set(app.name, app);
  }
}

if (flags.pkgUi) includePackage("@netmetric/ui");
if (flags.pkgConfig) includePackage("@netmetric/config");
if (flags.pkgTypes) includePackage("@netmetric/types");
if (flags.pkgLint) includePackage("@netmetric/eslint-config");
if (flags.pkgTsconfig) includePackage("@netmetric/tsconfig");
if (flags.pkgTailwind) includePackage("@netmetric/tailwind-config");

if (flags.appAccount) includeApp("account-web");
if (flags.appAdmin) includeApp("admin-web");
if (flags.appAuth) includeApp("auth-web");
if (flags.appCrm) includeApp("crm-web");
if (flags.appMobile) includeApp("mobile-app");
if (flags.appPublic) includeApp("public-web");
if (flags.appTools) includeApp("tools-web");

const packageChanged = [
  flags.pkgUi,
  flags.pkgConfig,
  flags.pkgTypes,
  flags.pkgLint,
  flags.pkgTsconfig,
  flags.pkgTailwind,
].some(Boolean);

if (packageChanged) {
  for (const app of allApps) {
    appSet.set(app.name, app);
  }
}

const appChanged = [
  flags.appAccount,
  flags.appAdmin,
  flags.appAuth,
  flags.appCrm,
  flags.appMobile,
  flags.appPublic,
  flags.appTools,
].some(Boolean);

const frontendChanged = infraTouched || packageChanged || appChanged;
const runDotnet = flags.dotnet || flags.tests || flags.tools || infraTouched;
const runNative = flags.native || flags.tests || flags.tools || infraTouched;
const runDeploy = flags.deploy || flags.tools || infraTouched;

const nonDocsChanged = frontendChanged || runDotnet || runNative || runDeploy;
const docsOnly = flags.docs && !nonDocsChanged;
const runFrontend = frontendChanged;

const packageMatrix = [...packageSet.values()];
const appMatrix = [...appSet.values()];

const packageCapabilities = {
  "@netmetric/ui": new Set(["lint", "typecheck", "build", "test", "coverage"]),
  "@netmetric/config": new Set(["typecheck", "build", "test", "coverage"]),
  "@netmetric/types": new Set(["typecheck", "build"]),
  "@netmetric/eslint-config": new Set(),
  "@netmetric/tsconfig": new Set(),
  "@netmetric/tailwind-config": new Set(),
};

const filterPackagesByTask = (task) =>
  packageMatrix.filter((pkg) => packageCapabilities[pkg.name]?.has(task));

const lintMatrix = filterPackagesByTask("lint");
const typecheckMatrix = filterPackagesByTask("typecheck");
const buildMatrix = filterPackagesByTask("build");
const testMatrix = filterPackagesByTask("test");
const coverageMatrix = filterPackagesByTask("coverage");

const lines = [
  `docs_only=${docsOnly}`,
  `run_frontend=${runFrontend}`,
  `run_dotnet=${runDotnet}`,
  `run_native=${runNative}`,
  `run_deploy=${runDeploy}`,
  `package_matrix=${JSON.stringify(packageMatrix)}`,
  `packages_lint_matrix=${JSON.stringify(lintMatrix)}`,
  `packages_typecheck_matrix=${JSON.stringify(typecheckMatrix)}`,
  `packages_build_matrix=${JSON.stringify(buildMatrix)}`,
  `packages_test_matrix=${JSON.stringify(testMatrix)}`,
  `packages_coverage_matrix=${JSON.stringify(coverageMatrix)}`,
  `app_matrix=${JSON.stringify(appMatrix)}`,
];

writeFileSync(outputsPath, `${lines.join("\n")}\n`, { encoding: "utf8", flag: "a" });
