import { execFileSync } from "node:child_process";
import { existsSync } from "node:fs";
import path from "node:path";

function resolveGitExecutable() {
  const candidates =
    process.platform === "win32"
      ? [String.raw`C:\Program Files\Git\cmd\git.exe`, String.raw`C:\Program Files\Git\bin\git.exe`]
      : ["/usr/bin/git", "/usr/local/bin/git"];

  return candidates.find((candidate) => existsSync(candidate)) ?? "git";
}

const gitExecutable = resolveGitExecutable();
const safePath = gitExecutable.includes(path.sep) ? path.dirname(gitExecutable) : process.env.PATH;
const safeEnv = { ...process.env, PATH: safePath };

const stagedFiles = execFileSync(
  gitExecutable,
  ["diff", "--cached", "--name-only", "--diff-filter=ACMR"],
  { encoding: "utf8", env: safeEnv },
)
  .split(/\r?\n/)
  .map((file) => file.trim())
  .filter(Boolean)
  .filter((file) => !file.endsWith(".env.example"));

if (stagedFiles.length === 0) {
  process.exit(0);
}

const patterns = [
  {
    name: "AWS access key",
    regex: /\bAKIA[0-9A-Z]{16}\b/,
  },
  {
    name: "Private key block",
    regex: /-----BEGIN (RSA|OPENSSH|EC|DSA|PGP) PRIVATE KEY-----/,
  },
  {
    name: "Known token/key format",
    regex:
      /\b(?:ghp_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|xox[baprs]-[A-Za-z0-9-]{20,}|sk-(?:live|test)?[A-Za-z0-9]{16,}|AIza[0-9A-Za-z\-_]{35}|ya29\.[0-9A-Za-z\-_]+)\b/,
  },
  {
    name: "Credentialed connection string",
    regex: /\b(?:postgres(?:ql)?|mysql|mssql|mongodb(?:\+srv)?|redis):\/\/[^:\s\/]+:[^@\s\/]+@/i,
  },
  {
    // Intentionally strict: secret-like key names must be assigned to a quoted literal.
    // This avoids false positives for form fields, schemas, route params, and FormData mappings.
    name: "Hardcoded secret literal assignment",
    regex:
      /\b(API[_-]?KEY|SECRET|TOKEN|PASSWORD|PRIVATE[_-]?KEY|CLIENT[_-]?SECRET|ACCESS[_-]?TOKEN|REFRESH[_-]?TOKEN)\b\s*[:=]\s*(["'`])([^"'`\r\n]*)\2/i,
  },
];

const falsePositiveAllowlist = [
  /\$env:SONAR_TOKEN\b/,
  /\$\{env:SONAR_TOKEN\}/,
  /\bprocess\.env\.SONAR_TOKEN\b/,
];

function isAllowlisted(line) {
  return falsePositiveAllowlist.some((pattern) => pattern.test(line));
}

function isScannerRuleDefinitionLine(file, line) {
  const normalizedFile = file.replaceAll("\\", "/");
  if (normalizedFile !== "scripts/security/scan-staged-secrets.mjs") {
    return false;
  }

  const selfRuleDefinitions = [
    /name:\s*"Hardcoded secret literal assignment"/,
    /\\b\(API\[_-\]\?KEY\|SECRET\|TOKEN\|PASSWORD\|PRIVATE\[_-\]\?KEY\)\\b/,
    /Known token\/key format/,
    /Credentialed connection string/,
  ];

  return selfRuleDefinitions.some((pattern) => pattern.test(line));
}

function isGenericFalsePositive(line) {
  const secretAssignmentMatch =
    /\b(API[_-]?KEY|SECRET|TOKEN|PASSWORD|PRIVATE[_-]?KEY|CLIENT[_-]?SECRET|ACCESS[_-]?TOKEN|REFRESH[_-]?TOKEN)\b\s*[:=]\s*(["'`])([^"'`\r\n]*)\2/i.exec(
      line,
    );

  if (!secretAssignmentMatch) {
    return false;
  }

  const literalValue = secretAssignmentMatch[3].trim();
  if (literalValue.length === 0) {
    return true;
  }

  // Safe placeholders/sample values should not fail pre-commit.
  if (
    /^(?:changeme|change_me|example|sample|placeholder|dummy|test|redacted|your[_-]?value)$/i.test(
      literalValue,
    )
  ) {
    return true;
  }

  // Non-secret text patterns commonly seen in app code.
  if (/^(?:auth|form|validation|route|i18n)(?:[.:/][A-Za-z0-9_.:/-]+)+$/i.test(literalValue)) {
    return true;
  }

  // Reject obvious expression-like values wrapped as strings.
  if (/\$\{[^}]+\}|\{[^}]+\}|formData\.get\(|process\.env\./i.test(literalValue)) {
    return true;
  }

  return !isSuspiciousSecretLiteral(literalValue);
}

function isSuspiciousSecretLiteral(value) {
  if (value.length < 8) {
    return false;
  }

  if (
    /^(?:ghp_|github_pat_|xox[baprs]-|sk-(?:live|test)?|AIza|ya29\.)/i.test(value) ||
    (/[A-Za-z]/.test(value) && /\d/.test(value) && /[^A-Za-z0-9]/.test(value))
  ) {
    return true;
  }

  // High-entropy heuristic for long opaque strings.
  if (value.length >= 20) {
    const entropy = calculateEntropy(value);
    return entropy >= 3.5;
  }

  return false;
}

function calculateEntropy(input) {
  const frequency = new Map();
  for (const char of input) {
    frequency.set(char, (frequency.get(char) ?? 0) + 1);
  }

  let entropy = 0;
  for (const count of frequency.values()) {
    const probability = count / input.length;
    entropy -= probability * Math.log2(probability);
  }

  return entropy;
}

const offenders = [];

for (const file of stagedFiles) {
  const content = execFileSync(gitExecutable, ["show", `:${file}`], {
    encoding: "utf8",
    env: safeEnv,
  });
  const lines = content.split(/\r?\n/);

  for (let index = 0; index < lines.length; index += 1) {
    const line = lines[index];
    for (const pattern of patterns) {
      if (pattern.regex.test(line)) {
        if (isAllowlisted(line)) {
          continue;
        }

        if (
          pattern.name === "Hardcoded secret literal assignment" &&
          isScannerRuleDefinitionLine(file, line)
        ) {
          continue;
        }

        if (
          pattern.name === "Hardcoded secret literal assignment" &&
          isGenericFalsePositive(line)
        ) {
          continue;
        }

        offenders.push({
          file,
          line: index + 1,
          reason: pattern.name,
        });
      }
    }
  }
}

if (offenders.length === 0) {
  process.exit(0);
}

console.error("Secret scanning failed. Potential secrets were found in staged changes:");

for (const offender of offenders) {
  console.error(`- ${offender.file}:${offender.line} (${offender.reason})`);
}

console.error("If this is a false positive, replace the value with a non-sensitive placeholder.");
process.exit(1);
