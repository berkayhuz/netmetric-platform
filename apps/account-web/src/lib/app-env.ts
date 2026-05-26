const LOCAL_DEFAULTS = {
  appOrigin: "http://localhost:7004",
  appName: "NetMetric Account",
  authUrl: "http://localhost:7002",
  accountUrl: "http://localhost:7004",
  crmUrl: "http://localhost:7006",
  toolsUrl: "http://localhost:7005",
  publicUrl: "http://localhost:7001",
} as const;

function getValue(value: string | undefined, fallback: string): string {
  const source = value?.trim() || fallback;
  return source.replace(/\/+$/, "");
}

export const appEnv = {
  appName: getValue(process.env.NEXT_PUBLIC_APP_NAME, LOCAL_DEFAULTS.appName),
  appOrigin: getValue(process.env.NEXT_PUBLIC_APP_ORIGIN, LOCAL_DEFAULTS.appOrigin),
  authUrl: getValue(process.env.NEXT_PUBLIC_AUTH_URL, LOCAL_DEFAULTS.authUrl),
  accountUrl: getValue(process.env.NEXT_PUBLIC_ACCOUNT_URL, LOCAL_DEFAULTS.accountUrl),
  crmUrl: getValue(process.env.NEXT_PUBLIC_CRM_URL, LOCAL_DEFAULTS.crmUrl),
  toolsUrl: getValue(process.env.NEXT_PUBLIC_TOOLS_URL, LOCAL_DEFAULTS.toolsUrl),
  publicUrl: getValue(process.env.NEXT_PUBLIC_PUBLIC_URL, LOCAL_DEFAULTS.publicUrl),
} as const;
