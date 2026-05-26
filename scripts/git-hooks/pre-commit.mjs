import { execFileSync, spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import path from "node:path";

function resolveExecutable(name) {
  const isWindows = process.platform === "win32";
  const candidates = isWindows
    ? {
        git: [
          String.raw`C:\Program Files\Git\cmd\git.exe`,
          String.raw`C:\Program Files\Git\bin\git.exe`,
        ],
        pnpm: [
          String.raw`C:\Program Files\nodejs\pnpm.cmd`,
          String.raw`C:\Program Files\nodejs\pnpm.exe`,
          path.join(process.env.APPDATA ?? "", "npm", "pnpm.cmd"),
        ],
      }
    : {
        git: ["/usr/bin/git", "/usr/local/bin/git"],
        pnpm: ["/usr/bin/pnpm", "/usr/local/bin/pnpm"],
      };

  const resolved = candidates[name]?.find((candidate) => existsSync(candidate));
  if (!resolved) {
    throw new Error(`Required executable not found in fixed locations: ${name}`);
  }
  return resolved;
}

const gitExecutable = resolveExecutable("git");
const pnpmExecutable = resolveExecutable("pnpm");
const safePathEntries = [
  path.dirname(gitExecutable),
  path.dirname(pnpmExecutable),
  path.dirname(process.execPath),
];
const safePath = Array.from(new Set(safePathEntries)).join(
  process.platform === "win32" ? ";" : ":",
);
const safeEnv = {
  ...process.env,
  PATH: safePath,
  ...(process.platform === "win32"
    ? {
        ComSpec: process.env.ComSpec ?? String.raw`C:\Windows\System32\cmd.exe`,
        SystemRoot: process.env.SystemRoot ?? String.raw`C:\Windows`,
        WINDIR: process.env.WINDIR ?? String.raw`C:\Windows`,
      }
    : {}),
};

function run(command, args) {
  if (process.platform === "win32" && command.toLowerCase().endsWith(".cmd")) {
    const result = spawnSync(command, args, {
      stdio: "inherit",
      env: safeEnv,
      shell: true,
      windowsHide: true,
    });
    if (typeof result.status === "number" && result.status === 0) {
      return;
    }
    throw new Error(`Command failed: ${command} ${args.join(" ")}`);
    return;
  }

  execFileSync(command, args, { stdio: "inherit", env: safeEnv });
}

const stagedFiles = execFileSync(
  gitExecutable,
  ["diff", "--cached", "--name-only", "--diff-filter=ACMR"],
  { encoding: "utf8", env: safeEnv },
)
  .split(/\r?\n/)
  .map((file) => file.trim())
  .filter(Boolean);

if (stagedFiles.length === 0) {
  process.exit(0);
}

run(pnpmExecutable, ["run", "guard:secrets"]);
run(pnpmExecutable, ["exec", "lint-staged"]);

const typecheckTargets = new Set();

for (const file of stagedFiles) {
  if (file.startsWith("packages/frontend/ui/")) {
    typecheckTargets.add("@netmetric/ui");
    continue;
  }

  if (file.startsWith("packages/frontend/config/")) {
    typecheckTargets.add("@netmetric/config");
    continue;
  }

  if (file.startsWith("packages/frontend/types/")) {
    typecheckTargets.add("@netmetric/types");
  }
}

if (typecheckTargets.size === 0) {
  process.exit(0);
}

for (const pkg of typecheckTargets) {
  run(pnpmExecutable, ["--filter", pkg, "typecheck"]);
}
