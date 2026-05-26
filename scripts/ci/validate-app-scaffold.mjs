import { existsSync, readFileSync } from "node:fs";
import path from "node:path";

const appPath = process.argv[2];

if (!appPath) {
  console.error("Usage: node scripts/ci/validate-app-scaffold.mjs <app-path>");
  process.exit(1);
}

const absoluteAppPath = path.resolve(process.cwd(), appPath);
const envExamplePath = path.join(absoluteAppPath, ".env.example");

if (!existsSync(absoluteAppPath)) {
  console.error(`App path does not exist: ${appPath}`);
  process.exit(1);
}

if (!existsSync(envExamplePath)) {
  console.error(`Missing .env.example in ${appPath}`);
  process.exit(1);
}

const content = readFileSync(envExamplePath, "utf8");

const requiredKeys = [
  "NODE_ENV=",
  "APP_ENV=",
  "NEXT_PUBLIC_APP_NAME=",
  "NEXT_PUBLIC_API_BASE_URL=",
  "NEXT_PUBLIC_APP_ORIGIN=",
];

for (const key of requiredKeys) {
  if (!content.includes(key)) {
    console.error(`${appPath}/.env.example is missing required key prefix: ${key}`);
    process.exit(1);
  }
}

console.log(`App scaffold validation passed for ${appPath}`);
