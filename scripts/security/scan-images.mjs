import { spawnSync } from "node:child_process";

const images = [
  { name: "public-web", dockerfile: "apps/public-web/Dockerfile" },
  { name: "account-web", dockerfile: "apps/account-web/Dockerfile" },
  { name: "crm-web", dockerfile: "apps/crm-web/Dockerfile" },
  { name: "tools-web", dockerfile: "apps/tools-web/Dockerfile" },
  { name: "auth-api", dockerfile: "services/auth/deploy/Dockerfile" },
  { name: "account-api", dockerfile: "services/account/src/NetMetric.Account.API/Dockerfile" },
  { name: "crm-api", dockerfile: "services/crm/src/NetMetric.CRM.API/Dockerfile" },
  { name: "tools-api", dockerfile: "services/tools/src/NetMetric.Tools.API/Dockerfile" },
  { name: "gateway", dockerfile: "platform/gateway/src/NetMetric.ApiGateway/Dockerfile" },
  {
    name: "notification-worker",
    dockerfile: "services/notification/src/NetMetric.Notification.Worker/Dockerfile",
  },
];

for (const image of images) {
  const tag = `netmetric/${image.name}:ci`;
  const build = spawnSync("docker", ["build", "-f", image.dockerfile, "-t", tag, "."], {
    stdio: "inherit",
    shell: true,
  });
  if (build.status !== 0) process.exit(build.status ?? 1);
  const scan = spawnSync(
    "trivy",
    [
      "image",
      "--severity",
      "HIGH,CRITICAL",
      "--exit-code",
      "1",
      "--format",
      "sarif",
      "-o",
      `trivy-${image.name}.sarif`,
      tag,
    ],
    { stdio: "inherit", shell: true },
  );
  if (scan.status !== 0) process.exit(scan.status ?? 1);
}
