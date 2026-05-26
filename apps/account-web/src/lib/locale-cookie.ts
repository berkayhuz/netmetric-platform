import { getPreferenceCookieOptions } from "@netmetric/i18n";

import { appEnv } from "./app-env";

export function resolveLocaleCookieOptions(config?: {
  appOrigin?: string;
  cookieDomain?: string | undefined;
}) {
  return getPreferenceCookieOptions({
    appOrigin: config?.appOrigin ?? appEnv.appOrigin,
    cookieDomain:
      config?.cookieDomain ??
      process.env.NETMETRIC_COOKIE_DOMAIN ??
      process.env.NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN,
  });
}
