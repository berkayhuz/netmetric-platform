const LOCAL_DEFAULTS = {
  siteUrl: "http://localhost:7005",
  appName: "NetMetric Tools",
  authUrl: "http://localhost:7002",
  toolsApiBaseUrl: "http://localhost:5305",
  apiBaseUrl: "http://localhost:8080",
} as const;

function readUrl(value: string | undefined, fallback: string): string {
  const source = value?.trim() || fallback;
  return source.replace(/\/+$/, "");
}

export const toolsEnv = {
  appName: process.env.NEXT_PUBLIC_APP_NAME?.trim() || LOCAL_DEFAULTS.appName,
  siteUrl: readUrl(process.env.NEXT_PUBLIC_APP_ORIGIN, LOCAL_DEFAULTS.siteUrl),
  authUrl: readUrl(process.env.NEXT_PUBLIC_AUTH_URL, LOCAL_DEFAULTS.authUrl),
  apiBaseUrl: readUrl(process.env.NEXT_PUBLIC_API_BASE_URL, LOCAL_DEFAULTS.apiBaseUrl),
  toolsApiBaseUrl: readUrl(process.env.TOOLS_API_BASE_URL, LOCAL_DEFAULTS.toolsApiBaseUrl),
} as const;

export function toAbsoluteUrl(pathname: string): string {
  const safePath = pathname.startsWith("/") ? pathname : `/${pathname}`;
  return `${toolsEnv.siteUrl}${safePath}`;
}
