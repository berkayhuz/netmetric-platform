import { existsSync, readdirSync } from "node:fs";
import path from "node:path";
import { spawnSync } from "node:child_process";

const task = process.argv[2];

if (!task) {
  console.error(
    "Usage: node scripts/orchestration/run-dotnet-task.mjs <restore|build|test|format>",
  );
  process.exit(1);
}

const rootDir = process.cwd();
const domainLabel = "dotnet";

const statusInfo = (status, message) => {
  console.log(`[${domainLabel}] STATUS=${status} ${message}`);
};

const statusError = (status, message) => {
  console.error(`[${domainLabel}] STATUS=${status} ${message}`);
};

const commandAvailable = (command) => {
  const check = spawnSync(command, ["--version"], { stdio: "pipe" });
  return check.status === 0;
};

function resolveDotnetExecutable() {
  const candidates =
    process.platform === "win32"
      ? [String.raw`C:\Program Files\dotnet\dotnet.exe`]
      : ["/usr/bin/dotnet", "/usr/local/bin/dotnet"];

  return candidates.find((candidate) => existsSync(candidate)) ?? null;
}

const collectFiles = (directory, predicate, found = []) => {
  if (!existsSync(directory)) {
    return found;
  }

  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      collectFiles(fullPath, predicate, found);
      continue;
    }

    if (entry.isFile() && predicate(fullPath)) {
      found.push(fullPath);
    }
  }

  return found;
};

const hasDotnetImplementationArtifacts = () => {
  const rootArtifacts = collectFiles(
    rootDir,
    (filePath) =>
      [".sln", ".slnx", ".csproj"].some((extension) =>
        filePath.toLowerCase().endsWith(extension),
      ) && path.dirname(filePath) === rootDir,
  );

  if (rootArtifacts.length > 0) {
    return true;
  }

  const domainRoots = ["services", "platform", path.join("packages", "dotnet")];
  for (const domainRoot of domainRoots) {
    const fullRoot = path.join(rootDir, domainRoot);
    const domainArtifacts = collectFiles(fullRoot, (filePath) =>
      [".csproj", ".sln", ".slnx", ".cs"].some((extension) =>
        filePath.toLowerCase().endsWith(extension),
      ),
    );
    if (domainArtifacts.length > 0) {
      return true;
    }
  }

  return false;
};

const pickDotnetTarget = () => {
  const explicitCandidates = ["Netmetric.slnx", "Netmetric.sln", "netmetric.slnx", "netmetric.sln"];
  for (const candidate of explicitCandidates) {
    if (existsSync(path.join(rootDir, candidate))) {
      return candidate;
    }
  }

  const entries = readdirSync(rootDir, { withFileTypes: true })
    .filter((entry) => entry.isFile())
    .map((entry) => entry.name);

  const slnx = entries.find((name) => name.toLowerCase().endsWith(".slnx"));
  if (slnx) {
    return slnx;
  }

  const sln = entries.find((name) => name.toLowerCase().endsWith(".sln"));
  if (sln) {
    return sln;
  }

  return null;
};

const target = pickDotnetTarget();

if (!target) {
  if (hasDotnetImplementationArtifacts()) {
    statusError(
      "blocked-missing-artifact",
      "Dotnet domain signals exist, but no solution file (.sln/.slnx) was found at repository root.",
    );
    process.exit(1);
  }

  statusInfo("skipped-intentional", "No dotnet implementation artifacts detected.");
  process.exit(0);
}

const commands = {
  restore: ["restore", target],
  build: ["build", target, "--configuration", "Release", "--nologo"],
  test: ["test", target, "--configuration", "Release", "--nologo", "--no-build"],
  format: ["format", target, "--verify-no-changes", "--no-restore"],
};

const args = commands[task];
if (!args) {
  console.error(`Unsupported dotnet task: ${task}`);
  process.exit(1);
}

const dotnetExecutable = resolveDotnetExecutable();
if (!dotnetExecutable && !commandAvailable("dotnet")) {
  statusError("blocked-missing-tooling", "dotnet CLI is required but not available on PATH.");
  process.exit(1);
}

statusInfo("executed", `Running dotnet ${args.join(" ")}`);
const safeCommand = dotnetExecutable ?? "dotnet";
const safePath = dotnetExecutable ? path.dirname(dotnetExecutable) : process.env.PATH;
const result = spawnSync(safeCommand, args, {
  stdio: "inherit",
  env: { ...process.env, PATH: safePath },
});
if (typeof result.status === "number") {
  process.exit(result.status);
}
statusError("blocked-missing-tooling", "Failed to execute dotnet process.");
process.exit(1);
