import fs from "node:fs";
import path from "node:path";

const repoRoot = process.cwd();
const scanRoots = ["docs", "scripts", "deploy", ".github", "README.md"];
const forbidden = [/src[\\/]Services[\\/]/i, /src[\\/]Gateway[\\/]/i];

const files = [];
function walk(target) {
  const full = path.join(repoRoot, target);
  if (!fs.existsSync(full)) return;
  const stat = fs.statSync(full);
  if (stat.isFile()) {
    files.push(full);
    return;
  }
  for (const entry of fs.readdirSync(full, { withFileTypes: true })) {
    if (["node_modules", ".git", ".next", "bin", "obj", ".runlogs"].includes(entry.name)) continue;
    walk(path.join(target, entry.name));
  }
}
scanRoots.forEach(walk);

const offenders = [];
for (const file of files) {
  const text = fs.readFileSync(file, "utf8");
  for (const pattern of forbidden) {
    if (pattern.test(text)) {
      offenders.push({
        file: path.relative(repoRoot, file).replaceAll("\\", "/"),
        pattern: pattern.toString(),
      });
    }
  }
}

if (offenders.length > 0) {
  for (const offender of offenders) {
    console.error(`[path-validation] ${offender.file} contains legacy pattern ${offender.pattern}`);
  }
  process.exit(1);
}

const requiredDocs = [
  "docs/operations/backup-restore-runbook.md",
  "docs/operations/environment-matrix.md",
  "docs/operations/domain-cookie-strategy.md",
  "apps/admin-web/README.md",
  "apps/mobile-app/README.md",
  "native/README.md",
];

const missing = requiredDocs.filter((p) => !fs.existsSync(path.join(repoRoot, p)));
if (missing.length > 0) {
  for (const item of missing) {
    console.error(`[path-validation] missing required file: ${item}`);
  }
  process.exit(1);
}

console.log("[path-validation] legacy path and required-doc checks passed.");
