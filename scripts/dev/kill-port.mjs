import { execFileSync } from "node:child_process";

const port = process.argv[2];

if (!port) {
  console.error("Usage: node scripts/dev/kill-port.mjs <port>");
  process.exit(1);
}

const isWindows = process.platform === "win32";

try {
  if (isWindows) {
    const output = execFileSync(
      "powershell.exe",
      [
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-Command",
        `
      $connections = Get-NetTCPConnection -LocalPort ${port} -ErrorAction SilentlyContinue;
      foreach ($connection in $connections) {
        Stop-Process -Id $connection.OwningProcess -Force -ErrorAction SilentlyContinue;
      }
      `,
      ],
      {
        stdio: "pipe",
        encoding: "utf8",
      },
    );

    if (output.trim()) {
      console.log(output.trim());
    }
  } else {
    execFileSync("sh", ["-c", `lsof -ti:${port} | xargs -r kill -9`], {
      stdio: "inherit",
    });
  }

  console.log(`Port ${port} stopped if it was in use.`);
} catch {
  console.log(`No process found on port ${port}, or it was already stopped.`);
}
