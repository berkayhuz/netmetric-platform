import { existsSync, readFileSync } from "node:fs";

const requiredFiles = [
  "services/auth/src/NetMetric.Auth.API/appsettings.Production.json",
  "services/account/src/NetMetric.Account.API/appsettings.Production.json",
  "services/tools/src/NetMetric.Tools.API/appsettings.Production.json",
  "services/crm/src/NetMetric.CRM.API/appsettings.Production.json",
  "services/notification/src/NetMetric.Notification.Worker/appsettings.Production.json",
];

const failures = [];
for (const file of requiredFiles) {
  if (!existsSync(file)) {
    failures.push(`${file}: missing`);
    continue;
  }
  const content = readFileSync(file, "utf8");
  if (/localhost|127\.0\.0\.1|example\.com/i.test(content)) {
    failures.push(`${file}: contains localhost/example placeholder`);
  }
  if (/UseInMemory|Development|DisableMigrations/i.test(content)) {
    failures.push(`${file}: contains unsafe production fallback`);
  }
}

if (failures.length > 0) {
  console.error(
    "Production config validation failed:\n" + failures.map((x) => `- ${x}`).join("\n"),
  );
  process.exit(1);
}

console.log("Production config validation passed.");
