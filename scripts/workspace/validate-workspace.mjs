import { existsSync, readFileSync } from "node:fs";
import path from "node:path";
import { workspaceDomainsConfig } from "./workspace-domains.config.mjs";

const rootDir = process.cwd();
const errors = [];
const packageNames = new Set();

const pushError = (message) => {
  errors.push(message);
};

const readJsonFile = (absolutePath) => {
  try {
    return JSON.parse(readFileSync(absolutePath, "utf8"));
  } catch {
    return null;
  }
};

const validateRoot = () => {
  for (const file of workspaceDomainsConfig.root.requiredFiles) {
    if (!existsSync(path.join(rootDir, file))) {
      pushError(`Missing required repository file: ${file}`);
    }
  }

  const rootPackagePath = path.join(rootDir, "package.json");
  if (!existsSync(rootPackagePath)) {
    return;
  }

  const rootPackage = readJsonFile(rootPackagePath);
  if (!rootPackage) {
    pushError("Root package.json is not valid JSON.");
    return;
  }

  if (
    typeof rootPackage.packageManager !== "string" ||
    !rootPackage.packageManager.startsWith(workspaceDomainsConfig.root.packageManagerPrefix)
  ) {
    pushError('Root package.json must define "packageManager" with a pinned pnpm version.');
  }
};

const validatePackageManifest = (relativePath, domainName) => {
  const absolutePath = path.join(rootDir, relativePath);
  if (!existsSync(absolutePath)) {
    pushError(`[${domainName}] Missing workspace package manifest: ${relativePath}`);
    return;
  }

  const pkg = readJsonFile(absolutePath);
  if (!pkg) {
    pushError(`[${domainName}] ${relativePath} is not valid JSON.`);
    return;
  }

  if (typeof pkg.name !== "string" || pkg.name.length === 0) {
    pushError(`[${domainName}] ${relativePath} is missing a valid "name" field.`);
    return;
  }

  if (packageNames.has(pkg.name)) {
    pushError(`[${domainName}] Duplicate package name detected: ${pkg.name}`);
  }
  packageNames.add(pkg.name);
};

const validateDomain = (domainName, config) => {
  for (const relativeDirectory of config.requiredDirectories ?? []) {
    if (!existsSync(path.join(rootDir, relativeDirectory))) {
      pushError(`[${domainName}] Missing required directory: ${relativeDirectory}`);
    }
  }

  const workspacePath = path.join(rootDir, "pnpm-workspace.yaml");
  if (existsSync(workspacePath)) {
    const workspaceText = readFileSync(workspacePath, "utf8");
    for (const requiredPattern of config.requiredWorkspacePatterns ?? []) {
      if (!workspaceText.includes(requiredPattern)) {
        pushError(`[${domainName}] pnpm-workspace.yaml must include ${requiredPattern}`);
      }
    }
  }

  for (const manifestPath of config.requiredPackageManifests ?? []) {
    validatePackageManifest(manifestPath, domainName);
  }
};

validateRoot();

for (const [domainName, domainConfig] of Object.entries(workspaceDomainsConfig.domains)) {
  validateDomain(domainName, domainConfig);
}

if (errors.length > 0) {
  console.error("Workspace validation failed:");
  for (const error of errors) {
    console.error(`- ${error}`);
  }
  process.exit(1);
}

console.log("Workspace validation passed.");
