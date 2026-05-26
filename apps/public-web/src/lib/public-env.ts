const LOCAL_DEFAULTS = {
  siteUrl: "http://localhost:7003",
  authUrl: "http://localhost:7002",
  accountUrl: "http://localhost:7004",
  crmUrl: "http://localhost:7006",
  toolsUrl: "http://localhost:7005",
  apiUrl: "http://localhost:5031",
  authSessionBaseUrl: "http://localhost:5030",
} as const;

function getUrl(value: string | undefined, fallback: string): string {
  const source = value?.trim() || fallback;
  return source.replace(/\/+$/, "");
}

export const publicEnv = {
  siteUrl: getUrl(process.env.NEXT_PUBLIC_SITE_URL, LOCAL_DEFAULTS.siteUrl),
  authUrl: getUrl(process.env.NEXT_PUBLIC_AUTH_URL, LOCAL_DEFAULTS.authUrl),
  accountUrl: getUrl(process.env.NEXT_PUBLIC_ACCOUNT_URL, LOCAL_DEFAULTS.accountUrl),
  crmUrl: getUrl(process.env.NEXT_PUBLIC_CRM_URL, LOCAL_DEFAULTS.crmUrl),
  toolsUrl: getUrl(process.env.NEXT_PUBLIC_TOOLS_URL, LOCAL_DEFAULTS.toolsUrl),
  apiUrl: getUrl(process.env.NEXT_PUBLIC_API_URL, LOCAL_DEFAULTS.apiUrl),
  authSessionBaseUrl: getUrl(
    process.env.NEXT_PUBLIC_AUTH_SESSION_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL,
    LOCAL_DEFAULTS.authSessionBaseUrl,
  ),
} as const;

export function toAbsoluteUrl(pathname: string): string {
  const safePath = pathname.startsWith("/") ? pathname : `/${pathname}`;
  return `${publicEnv.siteUrl}${safePath}`;
}
