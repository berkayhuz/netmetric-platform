import { existsSync } from "node:fs";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { readdirSync } from "node:fs";

const task = process.argv[2];
const rootDir = process.cwd();
const nativeDir = path.join(rootDir, "native");
const buildDir = path.join(nativeDir, "build");
const cmakeLists = path.join(nativeDir, "CMakeLists.txt");
const domainLabel = "native";

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

if (!task) {
  console.error("Usage: node scripts/orchestration/run-native-task.mjs <configure|build|test>");
  process.exit(1);
}

if (!existsSync(nativeDir)) {
  statusInfo("skipped-intentional", "No native directory found.");
  process.exit(0);
}

if (!existsSync(cmakeLists)) {
  const sourceArtifacts = collectFiles(nativeDir, (filePath) =>
    [".c", ".cc", ".cpp", ".cxx", ".h", ".hpp", ".hh"].some((extension) =>
      filePath.toLowerCase().endsWith(extension),
    ),
  );

  if (sourceArtifacts.length === 0) {
    statusInfo("skipped-intentional", "Native domain has no source artifacts yet.");
    process.exit(0);
  }

  statusError(
    "blocked-missing-artifact",
    "Native domain exists but native/CMakeLists.txt is missing.",
  );
  process.exit(1);
}

const run = (command, args) => {
  statusInfo("executed", `Running ${command} ${args.join(" ")}`);
  const result = spawnSync(command, args, { stdio: "inherit" });
  if (typeof result.status === "number") {
    return result.status;
  }
  return 1;
};

if (task === "configure") {
  if (!commandAvailable("cmake")) {
    statusError("blocked-missing-tooling", "cmake is required but not available on PATH.");
    process.exit(1);
  }
  process.exit(run("cmake", ["-S", nativeDir, "-B", buildDir]));
}

if (task === "build") {
  if (!existsSync(buildDir)) {
    statusError(
      "blocked-missing-artifact",
      "Native build directory is missing. Run native:configure first.",
    );
    process.exit(1);
  }
  if (!commandAvailable("cmake")) {
    statusError("blocked-missing-tooling", "cmake is required but not available on PATH.");
    process.exit(1);
  }
  process.exit(run("cmake", ["--build", buildDir, "--config", "Release"]));
}

if (task === "test") {
  if (!existsSync(buildDir)) {
    statusError(
      "blocked-missing-artifact",
      "Native build directory is missing. Run native:configure and native:build first.",
    );
    process.exit(1);
  }
  if (!commandAvailable("ctest")) {
    statusError("blocked-missing-tooling", "ctest is required but not available on PATH.");
    process.exit(1);
  }
  process.exit(run("ctest", ["--test-dir", buildDir, "--output-on-failure"]));
}

console.error(`Unsupported native task: ${task}`);
process.exit(1);
