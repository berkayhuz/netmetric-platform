import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const packageRoot = path.resolve(scriptDir, "..");
const messagesDir = path.join(packageRoot, "src", "messages");

const en = JSON.parse(fs.readFileSync(path.join(messagesDir, "en.json"), "utf8"));
const tr = JSON.parse(fs.readFileSync(path.join(messagesDir, "tr.json"), "utf8"));

const allowlistWords = new Set([
  "NetMetric",
  "CRM",
  "API",
  "MFA",
  "SLA",
  "SSL",
  "ID",
  "URL",
  "PNG",
  "JPG",
  "JPEG",
  "PDF",
  "JSON",
  "UUID",
  "workspace",
  "tenant",
  "cookie",
  "session",
  "auth",
  "account",
  "tools",
  "run",
  "runs",
]);

const genericPatterns = [
  /\b(?:Title|Description|Label|Placeholder)\b/iu,
  /\b(?:Fallback|Placeholder)\b/iu,
  /\b(?:Select\s+(?:policy|ticket|workspace|language|time\s*zone|valid))\b/iu,
];

const exactPlaceholderPatterns = [
  /^[A-Za-z ]+\s(?:Title|Description|Label|Placeholder)$/u,
  /^(?:Fallback|Placeholder)(?:\s[A-Za-z]+)*$/u,
];

const ignoredEnKeys = new Set([
  "crm.dashboard.summaryCardAria",
  "account.settings.openSection",
  "account.common.selectPlaceholder",
]);

const englishLeakPatterns = [
  /\b(?:label|placeholder|title|description|saved|deleted|detail|home|empty|select|confirm|search|only|failed|view)\b/iu,
  /\b(?:ticket|workspace|policy|rule|run|runs)\b/iu,
];

function isLikelyEnglishLeak(value) {
  if (!value || typeof value !== "string") return false;
  const tokens = value
    .split(/\s+/u)
    .map((t) => t.replace(/[^\p{L}\p{N}-]/gu, ""))
    .filter(Boolean);
  const candidate = tokens.filter((t) => t.length > 2 && !allowlistWords.has(t));
  if (candidate.length === 0) return false;
  if (!englishLeakPatterns.some((pattern) => pattern.test(value))) return false;
  return true;
}

const findings = [];
for (const [key, value] of Object.entries(en)) {
  if (ignoredEnKeys.has(key)) {
    continue;
  }
  if (
    typeof value === "string" &&
    (exactPlaceholderPatterns.some((pattern) => pattern.test(value.trim())) ||
      genericPatterns.some((pattern) => pattern.test(value)))
  ) {
    findings.push(`[i18n:quality:en] Generic copy in ${key}: ${JSON.stringify(value)}`);
  }
}

for (const [key, value] of Object.entries(tr)) {
  if (typeof value !== "string") continue;
  if (isLikelyEnglishLeak(value)) {
    findings.push(`[i18n:quality:tr] English residue in ${key}: ${JSON.stringify(value)}`);
  }
}

if (findings.length > 0) {
  for (const finding of findings.slice(0, 300)) {
    console.warn(finding);
  }
  if (findings.length > 300) {
    console.warn(`[i18n:quality] ${findings.length - 300} additional finding(s) omitted.`);
  }
  console.log(`[i18n:quality] Reported ${findings.length} potential quality issue(s).`);
} else {
  console.log("[i18n:quality] Generic/English residue checks passed.");
}
