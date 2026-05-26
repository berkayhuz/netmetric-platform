import { spawnSync } from "node:child_process";

const lint = spawnSync("kube-linter", ["lint", "deploy/kubernetes"], {
  stdio: "inherit",
  shell: true,
});
if (lint.status !== 0) process.exit(lint.status ?? 1);

const checkov = spawnSync(
  "checkov",
  ["-d", "deploy/kubernetes", "--quiet", "--framework", "kubernetes", "--soft-fail", "false"],
  { stdio: "inherit", shell: true },
);
process.exit(checkov.status ?? 1);
