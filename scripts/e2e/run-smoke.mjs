import { spawnSync } from "node:child_process";

const target = process.argv[2];
const requireRun = process.env.RUN_E2E_SMOKE === "1" || process.env.CI_E2E_SMOKE === "1";

if (!requireRun) {
  console.log("E2E smoke skipped. Set RUN_E2E_SMOKE=1 to execute.");
  process.exit(0);
}

const args = ["test", "tests/e2e/smoke.spec.ts", "--reporter=line"];
if (target) {
  process.env.NETMETRIC_E2E_TARGET = target;
}

const run = spawnSync("pnpm", ["exec", "playwright", ...args], {
  stdio: "inherit",
  shell: true,
  env: process.env,
});
process.exit(run.status ?? 1);
