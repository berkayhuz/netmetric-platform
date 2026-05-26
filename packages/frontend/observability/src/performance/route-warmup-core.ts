export type CriticalRouteWarmupRoute = Readonly<{
  href: string;
  label?: string;
}>;

const SIDE_EFFECT_PATH_PATTERN =
  /(^|\/)(api|logout|signout|sign-out|session-reset|revoke|delete|destroy|disable|reset|switch|impersonate)(\/|$)/i;

export function isWarmupSafeHref(href: string): boolean {
  const trimmed = href.trim();

  if (!trimmed || !trimmed.startsWith("/") || trimmed.startsWith("//")) {
    return false;
  }

  try {
    const url = new URL(trimmed, "https://netmetric.local");

    if (url.origin !== "https://netmetric.local") {
      return false;
    }

    return !SIDE_EFFECT_PATH_PATTERN.test(url.pathname);
  } catch {
    return false;
  }
}

export function createCriticalRouteWarmupConfig<
  const TRoutes extends readonly CriticalRouteWarmupRoute[],
>(routes: TRoutes): TRoutes {
  const unsafeRoutes = routes.filter((route) => !isWarmupSafeHref(route.href));

  if (unsafeRoutes.length > 0) {
    const unsafeList = unsafeRoutes.map((route) => route.href).join(", ");
    throw new Error(`Unsafe critical route warm-up href(s): ${unsafeList}`);
  }

  return routes;
}

export function getWarmupHrefs(
  routes: readonly CriticalRouteWarmupRoute[],
  currentPath: string | null | undefined,
  maxRoutes: number,
): string[] {
  const normalizedCurrentPath = normalizePath(currentPath ?? "/");
  const seen = new Set<string>();
  const hrefs: string[] = [];

  for (const route of routes) {
    const href = route.href.trim();
    const pathname = normalizePath(href);

    if (!isWarmupSafeHref(href) || pathname === normalizedCurrentPath || seen.has(href)) {
      continue;
    }

    seen.add(href);
    hrefs.push(href);

    if (hrefs.length >= maxRoutes) {
      break;
    }
  }

  return hrefs;
}

function normalizePath(path: string): string {
  try {
    return new URL(path, "https://netmetric.local").pathname.replace(/\/+$/, "") || "/";
  } catch {
    return path.replace(/\/+$/, "") || "/";
  }
}
