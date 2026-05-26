import { readdirSync, readFileSync } from "node:fs";
import path from "node:path";

const rootDir = process.cwd();
const uiSrcDir = path.join(rootDir, "packages", "frontend", "ui", "src");
const violations = [];

const isKebab = (value) => /^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(value);

const allowedRawColorFiles = new Set([
  "packages/frontend/ui/src/components/data-display/chart.tsx",
]);

const allowedVariantNames = new Set([
  "default",
  "destructive",
  "outline",
  "secondary",
  "ghost",
  "link",
]);

const walk = (dir, callback) => {
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const absolutePath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      walk(absolutePath, callback);
      continue;
    }
    callback(absolutePath);
  }
};

walk(path.join(uiSrcDir, "components"), (filePath) => {
  if (!filePath.endsWith(".tsx") && !filePath.endsWith(".ts")) {
    return;
  }

  const relativePath = path.relative(rootDir, filePath).replaceAll("\\", "/");
  const fileName = path.basename(filePath).replace(/\.(tsx|ts)$/, "");

  if (fileName !== "index" && !isKebab(fileName)) {
    violations.push(
      `Component naming rule violated: ${relativePath} must use kebab-case file name.`,
    );
  }

  const content = readFileSync(filePath, "utf8");

  if (content.includes('from "@/') || content.includes("from '@/")) {
    violations.push(
      `Alias policy violated: ${relativePath} must not use @/* alias inside UI package.`,
    );
  }

  if (
    !allowedRawColorFiles.has(relativePath) &&
    /#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})/.test(content)
  ) {
    violations.push(`Token rule violated: ${relativePath} contains raw hex colors.`);
  }
});

const buttonFile = readFileSync(
  path.join(uiSrcDir, "components", "primitives", "button.tsx"),
  "utf8",
);
for (const variantName of allowedVariantNames) {
  if (!buttonFile.includes(`${variantName}:`)) {
    violations.push(`Variant policy violated: button.tsx is missing '${variantName}' variant.`);
  }
}
if (!buttonFile.includes("defaultVariants")) {
  violations.push("Variant policy violated: button.tsx must define defaultVariants.");
}

const themeProviderFile = readFileSync(
  path.join(uiSrcDir, "components", "theme", "theme-provider.tsx"),
  "utf8",
);
if (!themeProviderFile.includes('classList.toggle("dark"')) {
  violations.push("Dark mode contract violated: ThemeProvider must toggle the 'dark' class.");
}
if (!themeProviderFile.includes("colorScheme")) {
  violations.push("Dark mode contract violated: ThemeProvider must set colorScheme.");
}

const tokensFile = readFileSync(path.join(uiSrcDir, "styles", "tokens.css"), "utf8");
for (const token of ["--nm-motion-fast", "--nm-ease-standard", "--nm-space-4", "--nm-radius"]) {
  if (!tokensFile.includes(token)) {
    violations.push(`Token contract violated: tokens.css missing ${token}.`);
  }
}

const globalsFile = readFileSync(path.join(uiSrcDir, "styles", "globals.css"), "utf8");
if (!globalsFile.includes("prefers-reduced-motion")) {
  violations.push(
    "Animation standard violated: globals.css must include reduced motion transition handling.",
  );
}
if (!globalsFile.includes(":focus-visible")) {
  violations.push(
    "Accessibility standard violated: globals.css must include focus-visible styling.",
  );
}

if (violations.length > 0) {
  console.error("UI governance validation failed:");
  for (const violation of violations) {
    console.error(`- ${violation}`);
  }
  process.exit(1);
}

console.log("UI governance validation passed.");
