import { spawnSync } from "node:child_process";
import {
  appendFileSync,
  existsSync,
  mkdirSync,
  readFileSync,
  readdirSync,
  statSync,
  writeFileSync,
} from "node:fs";
import http from "node:http";
import https from "node:https";
import path from "node:path";

const args = new Set(process.argv.slice(2));
const options = {
  runDevSmoke: args.has("--run-dev-smoke") || process.env.RELEASE_GATE_RUN_DEV_SMOKE === "1",
  skipSmoke: args.has("--skip-smoke") || process.env.RELEASE_GATE_SKIP_SMOKE === "1",
  ciOptionalSmoke:
    args.has("--ci-optional-smoke") || process.env.RELEASE_GATE_SMOKE_CI_OPTIONAL === "1",
  skipMigrationBundle:
    args.has("--skip-migration-bundle") || process.env.RELEASE_GATE_SKIP_MIGRATION_BUNDLE === "1",
  skipSecurityScan:
    args.has("--skip-security-scan") || process.env.RELEASE_GATE_SKIP_SECURITY_SCAN === "1",
  failOnWarn: args.has("--fail-on-warn") || process.env.RELEASE_GATE_FAIL_ON_WARN === "1",
  failFast: args.has("--fail-fast") || process.env.RELEASE_GATE_FAIL_FAST === "1",
};
const requiredSmokeTargets = [
  "services/auth/src/NetMetric.Auth.API/NetMetric.Auth.API.csproj",
  "services/account/src/NetMetric.Account.API/NetMetric.Account.API.csproj",
];

const repoRoot = process.cwd();
const timestamp = new Date().toISOString().replace(/[:.]/g, "-");
const runRoot = path.join(repoRoot, ".runlogs", "release-gate", timestamp);
const logsRoot = path.join(runRoot, "logs");
const artifactsRoot = path.join(runRoot, "artifacts");
const eventsPath = path.join(runRoot, "events.jsonl");
const summaryPath = path.join(runRoot, "summary.json");
mkdirSync(logsRoot, { recursive: true });
mkdirSync(artifactsRoot, { recursive: true });

const results = [];

function sanitizeName(value) {
  return value
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "");
}

function relativePath(value) {
  return path.relative(repoRoot, value).replaceAll(path.sep, "/");
}

function writeEvent(event) {
  appendFileSync(eventsPath, `${JSON.stringify(event)}\n`, "utf8");
}

function recordStep({ step, status, summary, startedAt, endedAt, logFile, details = {} }) {
  const result = {
    step,
    status,
    summary,
    durationMs: endedAt.getTime() - startedAt.getTime(),
    logFile: logFile ? relativePath(logFile) : null,
    details,
  };
  results.push(result);
  writeEvent({ type: "step", ...result, timestamp: endedAt.toISOString() });

  const logSuffix = result.logFile ? ` log=${result.logFile}` : "";
  console.log(`[${status}] ${step}: ${summary}${logSuffix}`);
}

function commandExists(command) {
  const probeArgs =
    process.platform === "win32" && command === "powershell"
      ? ["-NoProfile", "-Command", "$PSVersionTable.PSVersion.ToString()"]
      : ["--version"];
  const result = spawnSync(command, probeArgs, {
    encoding: "utf8",
    stdio: "pipe",
    shell: false,
  });
  return result.status === 0;
}

function resolvePowerShell() {
  if (commandExists("pwsh")) {
    return "pwsh";
  }
  if (process.platform === "win32" && commandExists("powershell")) {
    return "powershell";
  }
  return null;
}

function runCommand({ command, args: commandArgs, logFile, env = {}, timeoutMs = 0 }) {
  const startedAt = new Date();
  const result = spawnSync(command, commandArgs, {
    cwd: repoRoot,
    encoding: "utf8",
    env: { ...process.env, ...env },
    maxBuffer: 128 * 1024 * 1024,
    shell: false,
    timeout: timeoutMs,
  });
  const endedAt = new Date();

  const lines = [
    `$ ${command} ${commandArgs.join(" ")}`,
    `startedAt=${startedAt.toISOString()}`,
    `endedAt=${endedAt.toISOString()}`,
    `exitCode=${typeof result.status === "number" ? result.status : "null"}`,
    "",
    "----- stdout -----",
    result.stdout ?? "",
    "",
    "----- stderr -----",
    result.stderr ?? "",
  ];
  if (result.error) {
    lines.push("", "----- error -----", result.error.message);
  }
  writeFileSync(logFile, lines.join("\n"), "utf8");

  return {
    exitCode: typeof result.status === "number" ? result.status : 1,
    stdout: result.stdout ?? "",
    stderr: result.stderr ?? "",
    error: result.error,
    timedOut: result.error?.code === "ETIMEDOUT",
    startedAt,
    endedAt,
  };
}

function runRequiredCommandStep({ step, command, args: commandArgs, timeoutMs = 0 }) {
  const logFile = path.join(logsRoot, `${sanitizeName(step)}.log`);
  const startedAt = new Date();
  const result = runCommand({ command, args: commandArgs, logFile, timeoutMs });
  const endedAt = new Date();

  const status = result.exitCode === 0 ? "PASS" : "FAIL";
  const summary =
    result.exitCode === 0
      ? `${command} ${commandArgs.join(" ")} completed`
      : `${command} ${commandArgs.join(" ")} failed with exit code ${result.exitCode}`;

  recordStep({
    step,
    status,
    summary,
    startedAt,
    endedAt,
    logFile,
    details: { exitCode: result.exitCode },
  });
  return status;
}

function hasFailures() {
  return results.some((result) => result.status === "FAIL");
}

function shouldStopAfterFailure() {
  return options.failFast && hasFailures();
}

function collectFiles(root, predicate, found = []) {
  if (!existsSync(root)) {
    return found;
  }

  for (const entry of readdirSync(root, { withFileTypes: true })) {
    const fullPath = path.join(root, entry.name);
    if (entry.isDirectory()) {
      if (
        [".git", "node_modules", "bin", "obj", ".runlogs", ".turbo", ".next"].includes(entry.name)
      ) {
        continue;
      }
      collectFiles(fullPath, predicate, found);
      continue;
    }

    if (entry.isFile() && predicate(fullPath)) {
      found.push(fullPath);
    }
  }

  return found;
}

function runConfigValidation() {
  const step = "config validation";
  const startedAt = new Date();
  const logFile = path.join(logsRoot, "config-validation.log");
  const scanRoots = ["services", "platform", "apps"].map((segment) => path.join(repoRoot, segment));
  const jsonFiles = scanRoots.flatMap((root) =>
    collectFiles(root, (file) => /appsettings(?:\.[^.]+)?\.json$/i.test(path.basename(file))),
  );

  const errors = [];
  const warnings = [];
  const byDirectory = new Map();

  for (const file of jsonFiles) {
    const directory = path.dirname(file);
    const list = byDirectory.get(directory) ?? [];
    list.push(file);
    byDirectory.set(directory, list);

    try {
      const parsed = JSON.parse(readFileSync(file, "utf8"));
      const normalized = relativePath(file).toLowerCase();
      if (normalized.endsWith("appsettings.production.json")) {
        if (parsed?.Database?.ApplyMigrationsOnStartup === true) {
          errors.push(`${relativePath(file)} sets Database.ApplyMigrationsOnStartup=true`);
        }
        if (parsed?.Seed?.AllowProductionStartupSeed === true) {
          errors.push(`${relativePath(file)} sets Seed.AllowProductionStartupSeed=true`);
        }
        const raw = JSON.stringify(parsed);
        if (/NetMetric\.Dev|localhost|127\.0\.0\.1/i.test(raw)) {
          warnings.push(`${relativePath(file)} contains local-development host/value markers`);
        }
      }
    } catch (error) {
      errors.push(`${relativePath(file)} is not valid JSON: ${error.message}`);
    }
  }

  for (const [directory, files] of byDirectory.entries()) {
    const hasBase = files.some((file) => path.basename(file).toLowerCase() === "appsettings.json");
    const hasProduction = files.some(
      (file) => path.basename(file).toLowerCase() === "appsettings.production.json",
    );
    if (hasBase && !hasProduction && /(?:api|worker|gateway)/i.test(directory)) {
      warnings.push(
        `${relativePath(directory)} has appsettings.json without appsettings.Production.json`,
      );
    }
  }

  writeFileSync(
    logFile,
    [
      `Scanned ${jsonFiles.length} appsettings file(s).`,
      "",
      "Errors:",
      ...(errors.length > 0 ? errors.map((item) => `- ${item}`) : ["- none"]),
      "",
      "Warnings:",
      ...(warnings.length > 0 ? warnings.map((item) => `- ${item}`) : ["- none"]),
    ].join("\n"),
    "utf8",
  );

  const endedAt = new Date();
  const status = errors.length > 0 ? "FAIL" : warnings.length > 0 ? "WARN" : "PASS";
  const summary =
    errors.length > 0
      ? `${errors.length} config validation error(s)`
      : warnings.length > 0
        ? `${warnings.length} config validation warning(s)`
        : `${jsonFiles.length} appsettings file(s) validated`;

  recordStep({
    step,
    status,
    summary,
    startedAt,
    endedAt,
    logFile,
    details: { scanned: jsonFiles.length, errors, warnings },
  });
}

function runMigrationBundleValidation() {
  const step = "migration bundle validation";
  const startedAt = new Date();
  const logFile = path.join(logsRoot, "migration-bundle-validation.log");

  if (options.skipMigrationBundle) {
    writeFileSync(logFile, "Skipped by --skip-migration-bundle.\n", "utf8");
    recordStep({
      step,
      status: "SKIP",
      summary: "migration bundle validation skipped by option",
      startedAt,
      endedAt: new Date(),
      logFile,
    });
    return;
  }

  const targets = [
    {
      name: "auth",
      project:
        "services/auth/src/NetMetric.Auth.Infrastructure/NetMetric.Auth.Infrastructure.csproj",
      startupProject: "services/auth/src/NetMetric.Auth.API/NetMetric.Auth.API.csproj",
      context: "AuthDbContext",
      artifactName: "netmetric-auth-migrate",
    },
    {
      name: "account",
      project:
        "services/account/src/NetMetric.Account.Persistence/NetMetric.Account.Persistence.csproj",
      startupProject: "services/account/src/NetMetric.Account.API/NetMetric.Account.API.csproj",
      context: "AccountDbContext",
      artifactName: "netmetric-account-migrate",
    },
  ].filter(
    (target) =>
      existsSync(path.join(repoRoot, target.project)) &&
      existsSync(path.join(repoRoot, target.startupProject)),
  );

  if (targets.length === 0) {
    writeFileSync(logFile, "No known EF migration bundle targets were found.\n", "utf8");
    recordStep({
      step,
      status: "SKIP",
      summary: "no known EF migration bundle targets found",
      startedAt,
      endedAt: new Date(),
      logFile,
    });
    return;
  }

  const bundleRoot = path.join(artifactsRoot, "migration-bundles");
  mkdirSync(bundleRoot, { recursive: true });

  const toolLog = path.join(logsRoot, "migration-bundle-dotnet-tool-restore.log");
  const toolRestore = runCommand({
    command: "dotnet",
    args: ["tool", "restore"],
    logFile: toolLog,
  });
  const errors = [];
  const builtArtifacts = [];
  if (toolRestore.exitCode !== 0) {
    errors.push(`dotnet tool restore failed with exit code ${toolRestore.exitCode}`);
  } else {
    for (const target of targets) {
      const outputName =
        process.platform === "win32" ? `${target.artifactName}.exe` : target.artifactName;
      const outputPath = path.join(bundleRoot, outputName);
      const targetLog = path.join(logsRoot, `migration-bundle-${target.name}.log`);
      const result = runCommand({
        command: "dotnet",
        args: [
          "tool",
          "run",
          "dotnet-ef",
          "migrations",
          "bundle",
          "--project",
          target.project,
          "--startup-project",
          target.startupProject,
          "--context",
          target.context,
          "--configuration",
          "Release",
          "--no-build",
          "--force",
          "--output",
          outputPath,
        ],
        logFile: targetLog,
      });

      if (result.exitCode !== 0) {
        errors.push(`${target.name} bundle failed with exit code ${result.exitCode}`);
        continue;
      }

      if (!existsSync(outputPath) || statSync(outputPath).size === 0) {
        errors.push(`${target.name} bundle did not produce a non-empty artifact`);
        continue;
      }

      builtArtifacts.push({
        name: target.name,
        path: relativePath(outputPath),
        bytes: statSync(outputPath).size,
      });
    }
  }

  writeFileSync(
    logFile,
    [
      `Targets: ${targets.map((target) => target.name).join(", ")}`,
      `Tool restore log: ${relativePath(toolLog)}`,
      "",
      "Artifacts:",
      ...(builtArtifacts.length > 0
        ? builtArtifacts.map(
            (artifact) => `- ${artifact.name}: ${artifact.path} (${artifact.bytes} bytes)`,
          )
        : ["- none"]),
      "",
      "Errors:",
      ...(errors.length > 0 ? errors.map((item) => `- ${item}`) : ["- none"]),
    ].join("\n"),
    "utf8",
  );

  recordStep({
    step,
    status: errors.length > 0 ? "FAIL" : "PASS",
    summary:
      errors.length > 0
        ? `${errors.length} migration bundle error(s)`
        : `${builtArtifacts.length} migration bundle artifact(s) validated`,
    startedAt,
    endedAt: new Date(),
    logFile,
    details: { artifacts: builtArtifacts, errors },
  });
}

function runEfMigrationPolicyValidation() {
  const step = "ef migration policy";
  const startedAt = new Date();
  const logFile = path.join(logsRoot, "ef-migration-policy.log");
  const result = runCommand({
    command: "node",
    args: ["scripts/release/validate-ef-migrations.mjs"],
    logFile,
  });
  recordStep({
    step,
    status: result.exitCode === 0 ? "PASS" : "FAIL",
    summary:
      result.exitCode === 0
        ? "EF migration policy validated"
        : `EF migration policy failed with exit code ${result.exitCode}`,
    startedAt,
    endedAt: new Date(),
    logFile,
    details: { exitCode: result.exitCode },
  });
}

function httpGet(url, timeoutMs = 2500) {
  return new Promise((resolve) => {
    const client = url.startsWith("https:") ? https : http;
    const request = client.get(url, { timeout: timeoutMs }, (response) => {
      response.resume();
      response.on("end", () =>
        resolve({
          ok: response.statusCode >= 200 && response.statusCode < 400,
          statusCode: response.statusCode,
        }),
      );
    });
    request.on("timeout", () => {
      request.destroy();
      resolve({ ok: false, error: "timeout" });
    });
    request.on("error", (error) => resolve({ ok: false, error: error.message }));
  });
}

async function runHealthSmokeValidation() {
  const step = "health smoke";
  const startedAt = new Date();
  const logFile = path.join(logsRoot, "health-smoke.log");

  if (options.skipSmoke) {
    writeFileSync(logFile, "Skipped by --skip-smoke.\n", "utf8");
    recordStep({
      step,
      status: "SKIP",
      summary: "health smoke skipped by option",
      startedAt,
      endedAt: new Date(),
      logFile,
    });
    return;
  }

  const endpoints = [
    "http://localhost:5030/health/ready",
    "http://localhost:5030/auth/health/ready",
  ];
  const checks = [];
  for (const endpoint of endpoints) {
    checks.push({ endpoint, ...(await httpGet(endpoint)) });
  }

  writeFileSync(
    logFile,
    checks
      .map(
        (check) =>
          `${check.endpoint} ok=${check.ok} status=${check.statusCode ?? ""} error=${check.error ?? ""}`,
      )
      .join("\n"),
    "utf8",
  );

  const allReady = checks.every((check) => check.ok);
  recordStep({
    step,
    status: allReady ? "PASS" : "SKIP",
    summary: allReady
      ? "local gateway readiness endpoints responded successfully"
      : "local dev gateway is not running; full local smoke requires --run-dev-smoke",
    startedAt,
    endedAt: new Date(),
    logFile,
    details: { checks },
  });
}

function runFullLocalSmokeValidation() {
  const step = "full local smoke";
  const startedAt = new Date();
  const logFile = path.join(logsRoot, "full-local-smoke.log");

  if (options.skipSmoke) {
    writeFileSync(logFile, "Skipped by --skip-smoke.\n", "utf8");
    recordStep({
      step,
      status: "SKIP",
      summary: "full local smoke skipped by option",
      startedAt,
      endedAt: new Date(),
      logFile,
    });
    return;
  }

  const smokeScopeInRepo = requiredSmokeTargets.every((target) =>
    existsSync(path.join(repoRoot, target)),
  );
  const smokeRequired = smokeScopeInRepo;

  if (options.runDevSmoke) {
    const shell = resolvePowerShell();
    if (!shell) {
      writeFileSync(logFile, "PowerShell was not available for local smoke harness.\n", "utf8");
      recordStep({
        step,
        status: "FAIL",
        summary: "PowerShell is required for full local smoke",
        startedAt,
        endedAt: new Date(),
        logFile,
      });
      return;
    }

    const smokeScript = path.join(repoRoot, "scripts", "dev-smoke-auth-account.ps1");
    const smokeArgs = ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", smokeScript];
    if (options.ciOptionalSmoke) {
      smokeArgs.push("-CiOptional");
    }

    const result = runCommand({
      command: shell,
      args: smokeArgs,
      logFile,
    });
    const matrixPassed = /PASS matrix=auth-account-notification/i.test(result.stdout);
    const matrixSkipped =
      result.exitCode === 0 && /SKIP flow="Gateway reachability"/i.test(result.stdout);
    let status = result.exitCode === 0 ? (matrixSkipped ? "WARN" : "PASS") : "FAIL";
    if (smokeRequired && matrixSkipped) {
      status = "FAIL";
    }
    const summary =
      result.exitCode === 0
        ? matrixSkipped
          ? smokeRequired
            ? "Auth + Account + Notification local smoke is required but gateway was unreachable"
            : "Auth + Account + Notification local smoke skipped because local gateway was unreachable"
          : matrixPassed
            ? "Auth + Account + Notification local smoke matrix completed"
            : "Auth + Account + Notification local smoke completed without a PASS matrix marker"
        : `Auth + Account + Notification local smoke matrix failed with exit code ${result.exitCode}`;
    recordStep({
      step,
      status,
      summary,
      startedAt,
      endedAt: new Date(),
      logFile,
      details: {
        exitCode: result.exitCode,
        mode: "local-smoke",
        ciOptional: options.ciOptionalSmoke,
      },
    });
    return;
  }

  writeFileSync(
    logFile,
    smokeRequired
      ? "Required smoke failed because --run-dev-smoke was not provided.\n"
      : "Skipped because --run-dev-smoke was not provided.\n",
    "utf8",
  );
  recordStep({
    step,
    status: smokeRequired ? "FAIL" : "SKIP",
    summary: smokeRequired
      ? "Auth + Account + Notification local smoke is required for this release gate run"
      : "full local smoke requires --run-dev-smoke",
    startedAt,
    endedAt: new Date(),
    logFile,
    details: { mode: "local-smoke", smokeRequired },
  });
}

function runSecurityScan() {
  const step = "security scan";
  const startedAt = new Date();
  const logFile = path.join(logsRoot, "security-scan.log");

  if (options.skipSecurityScan) {
    writeFileSync(logFile, "Skipped by --skip-security-scan.\n", "utf8");
    recordStep({
      step,
      status: "SKIP",
      summary: "security scan skipped by option",
      startedAt,
      endedAt: new Date(),
      logFile,
    });
    return;
  }

  const secretScan = runCommand({
    command: "node",
    args: ["scripts/security/scan-staged-secrets.mjs"],
    logFile: path.join(logsRoot, "security-staged-secrets.log"),
  });
  const gitleaks = runCommand({
    command: "node",
    args: ["scripts/security/run-gitleaks-if-available.mjs"],
    logFile: path.join(logsRoot, "security-gitleaks.log"),
  });

  const errors = [];
  const warnings = [];
  if (secretScan.exitCode !== 0) {
    errors.push(`staged secret scan failed with exit code ${secretScan.exitCode}`);
  }
  if (gitleaks.exitCode !== 0) {
    errors.push(`gitleaks scan failed with exit code ${gitleaks.exitCode}`);
  }
  if (/Gitleaks is not installed locally/i.test(`${gitleaks.stdout}\n${gitleaks.stderr}`)) {
    const installHint =
      process.platform === "win32"
        ? "Install with `winget install -e --id Gitleaks.Gitleaks` and ensure `gitleaks --version` works."
        : "Install gitleaks and ensure `gitleaks --version` works.";
    const message = `gitleaks is not installed locally; gitleaks scan did not run. ${installHint}`;
    if (isCiEnvironment()) {
      errors.push(`${message}; CI release gate requires gitleaks`);
    } else {
      warnings.push(`${message}; local release gate records this as WARN`);
    }
  }

  writeFileSync(
    logFile,
    [
      `Staged secrets log: ${relativePath(path.join(logsRoot, "security-staged-secrets.log"))}`,
      `Gitleaks log: ${relativePath(path.join(logsRoot, "security-gitleaks.log"))}`,
      "",
      "Errors:",
      ...(errors.length > 0 ? errors.map((item) => `- ${item}`) : ["- none"]),
      "",
      "Warnings:",
      ...(warnings.length > 0 ? warnings.map((item) => `- ${item}`) : ["- none"]),
    ].join("\n"),
    "utf8",
  );

  recordStep({
    step,
    status: errors.length > 0 ? "FAIL" : warnings.length > 0 ? "WARN" : "PASS",
    summary:
      errors.length > 0
        ? `${errors.length} security scan error(s)`
        : warnings.length > 0
          ? `${warnings.length} security scan warning(s)`
          : "secret scans completed",
    startedAt,
    endedAt: new Date(),
    logFile,
    details: { errors, warnings },
  });
}

function isCiEnvironment() {
  return ["CI", "GITHUB_ACTIONS", "TF_BUILD", "BUILD_BUILDID"].some((name) => {
    const value = process.env[name];
    return value && value !== "0" && value.toLowerCase() !== "false";
  });
}

function runArtifactCheck() {
  const step = "artifact check";
  const startedAt = new Date();
  const logFile = path.join(logsRoot, "artifact-check.log");

  const git = spawnSync("git", ["ls-files"], {
    cwd: repoRoot,
    encoding: "utf8",
    maxBuffer: 64 * 1024 * 1024,
  });

  if (git.status !== 0) {
    writeFileSync(logFile, git.stderr ?? "git ls-files failed\n", "utf8");
    recordStep({
      step,
      status: "SKIP",
      summary: "git index is unavailable; artifact commit check skipped",
      startedAt,
      endedAt: new Date(),
      logFile,
    });
    return;
  }

  const blockedPattern =
    /(^|\/)(bin|obj|artifacts|\.artifacts|\.runlogs|testresults|node_modules|\.next|dist|generated)(\/|$)/i;
  const trackedFiles = git.stdout.split(/\r?\n/).filter(Boolean);
  const offenders = trackedFiles.filter((file) => blockedPattern.test(file.replaceAll("\\", "/")));
  const gitignore = existsSync(path.join(repoRoot, ".gitignore"))
    ? readFileSync(path.join(repoRoot, ".gitignore"), "utf8")
    : "";
  const ignoreChecks = [
    { name: ".runlogs", pattern: /\.runlogs\// },
    { name: "bin", pattern: /\[Bb\]in\// },
    { name: "obj", pattern: /\[Oo\]bj\// },
    { name: "artifacts", pattern: /(^|\n)artifacts\// },
  ];
  const missingIgnores = ignoreChecks
    .filter((check) => !check.pattern.test(gitignore))
    .map((check) => check.name);

  writeFileSync(
    logFile,
    [
      `Tracked files scanned: ${trackedFiles.length}`,
      "",
      "Tracked generated/artifact offenders:",
      ...(offenders.length > 0 ? offenders.map((item) => `- ${item}`) : ["- none"]),
      "",
      "Missing .gitignore coverage:",
      ...(missingIgnores.length > 0 ? missingIgnores.map((item) => `- ${item}`) : ["- none"]),
    ].join("\n"),
    "utf8",
  );

  const errors = [
    ...offenders.map((item) => `tracked generated/artifact path: ${item}`),
    ...missingIgnores.map((item) => `.gitignore does not cover ${item}`),
  ];

  recordStep({
    step,
    status: errors.length > 0 ? "FAIL" : "PASS",
    summary:
      errors.length > 0
        ? `${errors.length} artifact hygiene issue(s)`
        : "generated/bin/obj/artifact paths are not tracked and ignores are present",
    startedAt,
    endedAt: new Date(),
    logFile,
    details: { errors },
  });
}

function writeSummaryAndExit() {
  const failures = results.filter((result) => result.status === "FAIL");
  const warnings = results.filter((result) => result.status === "WARN");
  const skipped = results.filter((result) => result.status === "SKIP");
  const effectiveFailure = failures.length > 0 || (options.failOnWarn && warnings.length > 0);

  const summary = {
    status: effectiveFailure ? "FAIL" : warnings.length > 0 ? "WARN" : "PASS",
    runRoot: relativePath(runRoot),
    counts: {
      pass: results.filter((result) => result.status === "PASS").length,
      warn: warnings.length,
      fail: failures.length,
      skip: skipped.length,
    },
    results,
  };
  writeFileSync(summaryPath, JSON.stringify(summary, null, 2), "utf8");

  console.log("");
  console.log(`Release gate summary: ${summary.status}`);
  console.log(`Logs: ${relativePath(runRoot)}`);
  console.log(`Summary JSON: ${relativePath(summaryPath)}`);

  if (failures.length > 0 || (options.failOnWarn && warnings.length > 0)) {
    console.error("");
    console.error("Failure summary:");
    for (const failure of failures) {
      console.error(`- ${failure.step}: ${failure.summary} (${failure.logFile})`);
      const tail = readLogTail(failure.logFile);
      if (tail) {
        console.error(tail);
      }
    }
    if (options.failOnWarn) {
      for (const warning of warnings) {
        console.error(
          `- ${warning.step}: warning promoted to failure: ${warning.summary} (${warning.logFile})`,
        );
      }
    }
  }

  process.exit(effectiveFailure ? 1 : 0);
}

function readLogTail(relativeLogFile) {
  if (!relativeLogFile) {
    return "";
  }
  const fullPath = path.join(repoRoot, relativeLogFile);
  if (!existsSync(fullPath)) {
    return "";
  }
  const lines = readFileSync(fullPath, "utf8")
    .split(/\r?\n/)
    .filter((line) => line.trim().length > 0);
  const tail = lines.slice(-12);
  return tail.length > 0 ? tail.map((line) => `    ${line}`).join("\n") : "";
}

console.log(`NetMetric release gate started. Logs will be written to ${relativePath(runRoot)}`);
writeEvent({
  type: "run-start",
  timestamp: new Date().toISOString(),
  runRoot: relativePath(runRoot),
});

runRequiredCommandStep({
  step: "restore",
  command: "dotnet",
  args: ["restore", "NetMetric.slnx", "--force-evaluate"],
});
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
runRequiredCommandStep({
  step: "build",
  command: "dotnet",
  args: ["build", "NetMetric.slnx", "-c", "Release", "--no-restore", "-m:1", "-v", "minimal"],
});
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
runRequiredCommandStep({
  step: "test",
  command: "dotnet",
  args: ["test", "NetMetric.slnx", "-c", "Release", "--no-build", "-m:1", "-v", "minimal"],
});
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
runMigrationBundleValidation();
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
runEfMigrationPolicyValidation();
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
await runHealthSmokeValidation();
runFullLocalSmokeValidation();
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
runConfigValidation();
runRequiredCommandStep({
  step: "deployment surface validation",
  command: "node",
  args: ["scripts/release/validate-deployment-surface.mjs"],
});
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
runRequiredCommandStep({
  step: "path and docs validation",
  command: "node",
  args: ["scripts/workspace/validate-path-references.mjs"],
});
if (shouldStopAfterFailure()) {
  writeSummaryAndExit();
}
runSecurityScan();
runArtifactCheck();

writeEvent({ type: "run-end", timestamp: new Date().toISOString() });
writeSummaryAndExit();
