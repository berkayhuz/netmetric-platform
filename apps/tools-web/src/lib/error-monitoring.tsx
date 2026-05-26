"use client";

import { ErrorMonitoringBootstrap, createMonitoringConfig } from "@netmetric/observability";

const config = createMonitoringConfig({
  app: "tools-web",
  environment: process.env.NEXT_PUBLIC_APP_ENV ?? process.env.NODE_ENV,
  provider: process.env.NEXT_PUBLIC_ERROR_MONITORING_PROVIDER,
});

export function ToolsErrorMonitoring(): React.ReactElement {
  return <ErrorMonitoringBootstrap config={config} />;
}
