import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDir, "../../..");

const appRoots = ["auth-web", "account-web", "crm-web", "tools-web", "public-web"].map((name) =>
  path.join(repoRoot, "apps", name, "src"),
);

const findings = [];

function shouldInspect(filePath) {
  if (!/\.(ts|tsx)$/u.test(filePath)) return false;
  if (/\.test\.(ts|tsx)$/u.test(filePath)) return false;
  return true;
}

function visit(dirPath) {
  for (const entry of fs.readdirSync(dirPath, { withFileTypes: true })) {
    const fullPath = path.join(dirPath, entry.name);
    if (entry.isDirectory()) {
      visit(fullPath);
      continue;
    }

    if (!shouldInspect(fullPath)) continue;

    const source = fs.readFileSync(fullPath, "utf8");
    const lines = source.split(/\r?\n/u);
    for (let index = 0; index < lines.length; index += 1) {
      const line = lines[index];
      if (/toast\.(?:success|error|warning|info)\(\s*["'`]/u.test(line)) {
        findings.push(`[action-i18n] ${fullPath}:${index + 1} contains literal toast text.`);
      }

      if (/status:\s*["'](?:error|success)["']\s*,\s*message:\s*["'`]/u.test(line)) {
        findings.push(
          `[action-i18n] ${fullPath}:${index + 1} contains literal action result message.`,
        );
      }
    }
  }
}

for (const root of appRoots) {
  if (fs.existsSync(root)) {
    visit(root);
  }
}

if (findings.length > 0) {
  for (const finding of findings.slice(0, 200)) {
    console.warn(finding);
  }
  if (findings.length > 200) {
    console.warn(`[action-i18n] ${findings.length - 200} additional finding(s) omitted.`);
  }
  process.exitCode = 1;
} else {
  console.log("[action-i18n] No literal action/toast user messages found in app sources.");
}
