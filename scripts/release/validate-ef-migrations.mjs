import { existsSync, readdirSync } from "node:fs";
import path from "node:path";

const repoRoot = process.cwd();

const requiredTargets = [
  {
    name: "auth",
    project: "services/auth/src/NetMetric.Auth.Infrastructure/NetMetric.Auth.Infrastructure.csproj",
    migrationsDir: "services/auth/src/NetMetric.Auth.Infrastructure/Migrations",
  },
  {
    name: "account",
    project:
      "services/account/src/NetMetric.Account.Persistence/NetMetric.Account.Persistence.csproj",
    migrationsDir: "services/account/src/NetMetric.Account.Persistence/Migrations",
  },
];

const errors = [];

for (const target of requiredTargets) {
  const projectPath = path.join(repoRoot, target.project);
  if (!existsSync(projectPath)) {
    continue;
  }

  const migrationsPath = path.join(repoRoot, target.migrationsDir);
  if (!existsSync(migrationsPath)) {
    errors.push(
      `[${target.name}] required migrations directory is missing: ${target.migrationsDir}`,
    );
    continue;
  }

  const entries = readdirSync(migrationsPath, { withFileTypes: true })
    .filter((entry) => entry.isFile())
    .map((entry) => entry.name);
  const hasSnapshot = entries.some((file) => file.endsWith("ModelSnapshot.cs"));
  const migrationFiles = entries.filter((file) => /^\d{14}_.+\.cs$/i.test(file));

  if (!hasSnapshot) {
    errors.push(`[${target.name}] ModelSnapshot file is missing in ${target.migrationsDir}`);
  }
  if (migrationFiles.length === 0) {
    errors.push(`[${target.name}] no timestamped migration files found in ${target.migrationsDir}`);
  }
}

if (errors.length > 0) {
  console.error("EF migration policy validation failed:");
  for (const error of errors) {
    console.error(`- ${error}`);
  }
  process.exit(1);
}

console.log("EF migration policy validation passed.");
