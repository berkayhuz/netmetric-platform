import fs from "node:fs";
import path from "node:path";

const coveragePath = path.resolve(process.cwd(), "coverage", "lcov.info");

if (!fs.existsSync(coveragePath)) {
  console.error("[ui-coverage-targets] coverage/lcov.info was not found.");
  process.exit(1);
}

const targets = [
  { name: "Button", file: String.raw`src\components\primitives\button.tsx` },
  { name: "Input", file: String.raw`src\components\primitives\input.tsx` },
  { name: "Select", file: String.raw`src\components\primitives\select.tsx` },
  { name: "Dialog", file: String.raw`src\components\overlay\dialog.tsx` },
  { name: "Dropdown", file: String.raw`src\components\overlay\dropdown-menu.tsx` },
  { name: "Sidebar", file: String.raw`src\components\navigation\sidebar.tsx` },
  { name: "ThemeProvider", file: String.raw`src\components\theme\theme-provider.tsx` },
  { name: "DataGrid", file: String.raw`src\components\data-display\data-grid\data-grid.tsx` },
];

const lcov = fs.readFileSync(coveragePath, "utf8");
const records = lcov.split("end_of_record");

const findRecord = (targetPath) =>
  records.find((record) => record.includes(`SF:${targetPath}`) || record.includes(targetPath));

const failures = [];

for (const target of targets) {
  const record = findRecord(target.file);
  if (!record) {
    failures.push(`${target.name}: missing in lcov (${target.file})`);
    continue;
  }

  const lfMatch = record.match(/LF:(\d+)/);
  const lhMatch = record.match(/LH:(\d+)/);
  const lf = lfMatch ? Number(lfMatch[1]) : 0;
  const lh = lhMatch ? Number(lhMatch[1]) : 0;

  if (lf === 0 || lh === 0) {
    failures.push(`${target.name}: uncovered target (LH=${lh}, LF=${lf})`);
    continue;
  }

  const pct = ((lh / lf) * 100).toFixed(1);
  console.log(`[ui-coverage-targets] ${target.name} ${lh}/${lf} (${pct}%)`);
}

if (failures.length > 0) {
  console.error("[ui-coverage-targets] Coverage target validation failed:");
  for (const failure of failures) {
    console.error(`- ${failure}`);
  }
  process.exit(1);
}

console.log("[ui-coverage-targets] All target components are covered.");
