import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDir, "../../../..");
const packageRoot = path.resolve(scriptDir, "..");
const messagesDir = path.join(packageRoot, "src", "messages");
const appRoots = [
  "apps/auth-web/src",
  "apps/account-web/src",
  "apps/crm-web/src",
  "apps/tools-web/src",
  "apps/public-web/src",
];
const sourceExtensions = new Set([".ts", ".tsx", ".js", ".jsx"]);
const mojibakePattern = new RegExp(
  ["Hesab\\u00c4", "giri\\u00c5", "\\u00c4\\u00b1", "\\u00c5", "\\u00c3", "\\u00c2"].join("|"),
  "u",
);

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8"));
}

function walk(dirPath, files = []) {
  if (!fs.existsSync(dirPath)) return files;

  for (const entry of fs.readdirSync(dirPath, { withFileTypes: true })) {
    if (entry.name === ".next" || entry.name === "node_modules") continue;

    const fullPath = path.join(dirPath, entry.name);
    if (entry.isDirectory()) {
      walk(fullPath, files);
    } else if (sourceExtensions.has(path.extname(entry.name))) {
      files.push(fullPath);
    }
  }

  return files;
}

function compareKeys(leftName, left, rightName, right) {
  const rightKeys = new Set(Object.keys(right));
  return Object.keys(left)
    .filter((key) => !rightKeys.has(key))
    .map((key) => `${key} is present in ${leftName} but missing in ${rightName}`);
}

function isAllowedLiteral(value) {
  const normalized = value.trim();
  if (!normalized) return true;
  if (normalized.length < 2) return true;
  if (/^(NetMetric|NM|CRM|API|MFA|SLA|SSL|ID|URL|PNG|JPG|JPEG|PDF|JSON|UUID)$/u.test(normalized))
    return true;
  if (
    /^(Promise|delete-tool-run|GET|POST|PUT|PATCH|DELETE|Enter|Escape|Tab|Shift|Ctrl|Alt)$/u.test(
      normalized,
    )
  )
    return true;
  if (/^(https?:\/\/|\/|#|[A-Z_]+$)/u.test(normalized)) return true;
  if (/^[a-z0-9.-]+\.[a-z]{2,}$/iu.test(normalized)) return true;
  if (/[;{}()]|=>|return\s*\(/u.test(normalized)) return true;
  if (/^[\w-]+\/[\w.+-]+$/u.test(normalized)) return true;
  if (/^[\d\s.,:;+\-_%(){}[\]×]+$/u.test(normalized)) return true;
  if (/^(use client|server-only)$/u.test(normalized)) return true;
  return false;
}

function collectSuspiciousStrings(filePath, text) {
  const suspicious = [];
  const checks = [
    { kind: "jsx-text", regex: />\s*([A-Za-zÇĞİÖŞÜçğıöşü][^<>{}]{1,})\s*</gu },
    {
      kind: "attr",
      regex:
        /\b(?:placeholder|aria-label|title|description|label|caption|emptyTitle|emptyDescription|alt)=["']([A-Za-zÇĞİÖŞÜçğıöşü][^"']{1,})["']/gu,
    },
  ];

  for (const { kind, regex } of checks) {
    for (const match of text.matchAll(regex)) {
      const value = match[1].replace(/\s+/gu, " ").trim();
      if (isAllowedLiteral(value)) continue;
      const line = text.slice(0, match.index).split(/\r?\n/u).length;
      suspicious.push({ file: path.relative(repoRoot, filePath), line, kind, value });
    }
  }

  return suspicious;
}

const en = readJson(path.join(messagesDir, "en.json"));
const tr = readJson(path.join(messagesDir, "tr.json"));
const parityErrors = [...compareKeys("en", en, "tr", tr), ...compareKeys("tr", tr, "en", en)];

const sourceFiles = appRoots.flatMap((appRoot) => walk(path.join(repoRoot, appRoot)));
const mojibakeFindings = [];
const suspicious = [];

for (const filePath of sourceFiles) {
  const text = fs.readFileSync(filePath, "utf8");
  if (mojibakePattern.test(text)) {
    mojibakeFindings.push(path.relative(repoRoot, filePath));
  }
  suspicious.push(...collectSuspiciousStrings(filePath, text));
}

if (parityErrors.length > 0 || mojibakeFindings.length > 0) {
  for (const error of parityErrors) console.error(`[i18n:parity] ${error}`);
  for (const file of mojibakeFindings)
    console.error(`[i18n:encoding] Possible mojibake in ${file}`);
  process.exit(1);
}

if (suspicious.length > 0) {
  console.warn(
    `[i18n:report] ${suspicious.length} suspicious literals found. Review before release:`,
  );
  for (const item of suspicious.slice(0, 100)) {
    console.warn(`${item.file}:${item.line} ${item.kind}: ${JSON.stringify(item.value)}`);
  }
  if (suspicious.length > 100) {
    console.warn(`[i18n:report] ${suspicious.length - 100} additional findings omitted.`);
  }
} else {
  console.log("[i18n:report] No suspicious hardcoded UI literals found.");
}

console.log("[i18n:parity] en and tr dictionaries have matching key coverage.");
