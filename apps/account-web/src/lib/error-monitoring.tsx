"use client";

import { ErrorMonitoringBootstrap, createMonitoringConfig } from "@netmetric/observability";

const config = createMonitoringConfig({
  app: "account-web",
  environment: process.env.NEXT_PUBLIC_APP_ENV ?? process.env.NODE_ENV,
  provider: process.env.NEXT_PUBLIC_ERROR_MONITORING_PROVIDER,
});

export function AccountErrorMonitoring(): React.ReactElement {
  return <ErrorMonitoringBootstrap config={config} />;
}
