import { existsSync, readFileSync } from "node:fs";

const frontendTargets = [
  { name: "crm-web", path: "apps/crm-web/coverage/coverage-summary.json", min: 0.2 },
  { name: "public-web", path: "apps/public-web/coverage/coverage-summary.json", min: 0.2 },
  { name: "tools-web", path: "apps/tools-web/coverage/coverage-summary.json", min: 0.2 },
];

function getPct(path) {
  const json = JSON.parse(readFileSync(path, "utf8"));
  return Number(json.total?.lines?.pct ?? 0) / 100;
}

const fails = [];
for (const target of frontendTargets) {
  if (!existsSync(target.path)) {
    fails.push(`${target.name}: missing coverage report at ${target.path}`);
    continue;
  }
  const pct = getPct(target.path);
  if (pct < target.min) {
    fails.push(
      `${target.name}: lines ${(pct * 100).toFixed(2)}% < ${(target.min * 100).toFixed(0)}%`,
    );
  }
}

const dotnetSummary = "TestResults/dotnet/coverage-summary.json";
if (!existsSync(dotnetSummary)) {
  fails.push(`dotnet: missing coverage summary at ${dotnetSummary}`);
} else {
  const pct = Number(JSON.parse(readFileSync(dotnetSummary, "utf8")).lineRate ?? 0);
  if (pct < 0.2) {
    fails.push(`dotnet: lines ${(pct * 100).toFixed(2)}% < 20%`);
  }
}

if (fails.length > 0) {
  console.error("Coverage threshold failed:\n" + fails.map((x) => `- ${x}`).join("\n"));
  process.exit(1);
}

console.log("Coverage threshold passed.");
