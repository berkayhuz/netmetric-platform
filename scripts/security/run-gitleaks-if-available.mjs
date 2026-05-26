import { spawnSync } from "node:child_process";
import { existsSync, readdirSync } from "node:fs";

function resolveGitleaksExecutable() {
  const candidates = [];

  if (process.env.GITLEAKS_BIN) {
    candidates.push(process.env.GITLEAKS_BIN);
  }

  if (process.platform === "win32") {
    candidates.push(String.raw`C:\Program Files\gitleaks\gitleaks.exe`);

    const localAppData = process.env.LOCALAPPDATA;
    if (localAppData) {
      candidates.push(`${localAppData}\\Microsoft\\WinGet\\Links\\gitleaks.exe`);

      const packagesDir = `${localAppData}\\Microsoft\\WinGet\\Packages`;
      if (existsSync(packagesDir)) {
        for (const entry of readdirSync(packagesDir, { withFileTypes: true })) {
          if (!entry.isDirectory() || !entry.name.toLowerCase().includes("gitleaks")) {
            continue;
          }
          candidates.push(`${packagesDir}\\${entry.name}\\gitleaks.exe`);
        }
      }
    }

    const whereResult = spawnSync("where.exe", ["gitleaks"], { encoding: "utf8", stdio: "pipe" });
    if (whereResult.status === 0) {
      for (const line of (whereResult.stdout ?? "").split(/\r?\n/).map((value) => value.trim())) {
        if (line.length > 0) {
          candidates.push(line);
        }
      }
    }
  } else {
    candidates.push("/usr/bin/gitleaks", "/usr/local/bin/gitleaks");
  }

  for (const candidate of candidates) {
    if (candidate && existsSync(candidate)) {
      return candidate;
    }
  }

  return "gitleaks";
}

const gitleaksExecutable = resolveGitleaksExecutable();
const runtimeEnv = { ...process.env };

const versionCheck = spawnSync(gitleaksExecutable, ["version"], {
  encoding: "utf8",
  env: runtimeEnv,
  stdio: "pipe",
});

if (versionCheck.status !== 0) {
  console.log(
    [
      "Gitleaks is not installed locally. Skipping local gitleaks scan.",
      `Resolution target: ${gitleaksExecutable}`,
      `PATH (runtime): ${process.env.PATH ?? "<empty>"}`,
      `version stderr: ${(versionCheck.stderr ?? "").trim() || "<empty>"}`,
    ].join("\n"),
  );
  process.exit(0);
}

const result = spawnSync(
  gitleaksExecutable,
  ["detect", "--source=.", "--config=.gitleaks.toml", "--no-banner", "--redact", "--exit-code=1"],
  { stdio: "inherit", env: runtimeEnv },
);

if (typeof result.status === "number") {
  process.exit(result.status);
}

process.exit(1);
