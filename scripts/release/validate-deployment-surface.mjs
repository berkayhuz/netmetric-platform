import fs from "node:fs";
import path from "node:path";

const root = process.cwd();
const errors = [];

function read(p) {
  return fs.readFileSync(path.join(root, p), "utf8");
}
function exists(p) {
  return fs.existsSync(path.join(root, p));
}

const dockerfiles = [
  "apps/account-web/Dockerfile",
  "apps/crm-web/Dockerfile",
  "apps/tools-web/Dockerfile",
  "apps/public-web/Dockerfile",
  "services/account/src/NetMetric.Account.API/Dockerfile",
  "services/crm/src/NetMetric.CRM.API/Dockerfile",
  "platform/gateway/src/NetMetric.ApiGateway/Dockerfile",
  "services/notification/src/NetMetric.Notification.Worker/Dockerfile",
];
for (const file of dockerfiles) {
  if (!exists(file)) {
    errors.push(`missing file: ${file}`);
    continue;
  }
  const text = read(file);
  if (/COPY --from=build\s+\/workspace\s+\/workspace/.test(text))
    errors.push(`${file} copies full workspace into runtime`);
  if (/src\/Services\//i.test(text) || /src\/Gateway\//i.test(text))
    errors.push(`${file} contains legacy path`);
}

const kubeFiles = fs
  .readdirSync(path.join(root, "deploy/kubernetes"), { recursive: true })
  .filter((f) => f.toString().endsWith(".yaml"))
  .map((f) => path.join("deploy/kubernetes", f.toString().replaceAll("\\", "/")));

for (const file of kubeFiles) {
  const text = read(file);
  if (text.includes("REPLACE_WITH_GIT_SHA")) errors.push(`${file} still uses REPLACE_WITH_GIT_SHA`);
  if (
    text.includes("kind: Deployment") &&
    text.includes("image:") &&
    !text.includes("${IMAGE_TAG}")
  ) {
    errors.push(`${file} deployment image does not use \${IMAGE_TAG}`);
  }
  if (text.includes("kind: Deployment") && !text.includes("securityContext:")) {
    errors.push(`${file} missing securityContext`);
  }
}

if (exists("deploy/kubernetes/tools/tools-api-deployment.yaml")) {
  const tools = read("deploy/kubernetes/tools/tools-api-deployment.yaml");
  if (!tools.includes("persistentVolumeClaim"))
    errors.push("tools-api deployment missing PVC for artifacts");
}

const portChecks = [
  ["services/auth/src/NetMetric.Auth.API/appsettings.Production.json", "http://+:8080"],
  ["services/account/src/NetMetric.Account.API/appsettings.Production.json", "http://+:8080"],
  ["services/crm/src/NetMetric.CRM.API/appsettings.Production.json", "http://+:8080"],
  ["services/tools/src/NetMetric.Tools.API/appsettings.Production.json", "http://+:8080"],
  ["platform/gateway/src/NetMetric.ApiGateway/appsettings.Production.json", "http://+:8080"],
];
for (const [file, expected] of portChecks) {
  const text = read(file);
  if (!text.includes(expected)) errors.push(`${file} missing ${expected}`);
}

const prodSettings = [
  "services/auth/src/NetMetric.Auth.API/appsettings.Production.json",
  "services/account/src/NetMetric.Account.API/appsettings.Production.json",
  "services/notification/src/NetMetric.Notification.Worker/appsettings.Production.json",
];
for (const f of prodSettings) {
  if (read(f).includes('"ApplyMigrationsOnStartup": true'))
    errors.push(`${f} enables startup migrations in production`);
}

if (errors.length > 0) {
  for (const e of errors) console.error(`[deploy-validation] ${e}`);
  process.exit(1);
}

console.log("[deploy-validation] deployment surface checks passed.");
