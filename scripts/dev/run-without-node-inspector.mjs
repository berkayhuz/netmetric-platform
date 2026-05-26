import { spawn } from "node:child_process";

const inspectorOptionPattern = /(?:^|\s)--inspect(?:-brk|-port)?(?:=\S+)?/g;

function stripInspectorOptions(value) {
  if (!value) {
    return value;
  }

  const stripped = value.replace(inspectorOptionPattern, " ").replace(/\s+/g, " ").trim();
  return stripped.length > 0 ? stripped : undefined;
}

const [command, ...args] = process.argv.slice(2);

if (!command) {
  console.error("Usage: node scripts/dev/run-without-node-inspector.mjs <command> [...args]");
  process.exit(1);
}

const env = { ...process.env };

const sanitizedNodeOptions = stripInspectorOptions(env.NODE_OPTIONS);

if (sanitizedNodeOptions) {
  env.NODE_OPTIONS = sanitizedNodeOptions;
} else {
  delete env.NODE_OPTIONS;
}

delete env.VSCODE_INSPECTOR_OPTIONS;
delete env.NODE_INSPECT_RESUME_ON_START;

function spawnCommand(command, args, options) {
  if (process.platform !== "win32") {
    return spawn(command, args, options);
  }

  const executable = command === "pnpm" ? "pnpm.cmd" : command;

  return spawn("cmd.exe", ["/d", "/s", "/c", executable, ...args], options);
}

const child = spawnCommand(command, args, {
  env,
  stdio: "inherit",
});

child.on("error", (error) => {
  console.error(`[run-without-node-inspector] Failed to start "${command}": ${error.message}`);
  process.exit(1);
});

child.on("exit", (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }

  process.exit(code ?? 0);
});
