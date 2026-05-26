export type NextRemotePattern = {
  protocol: "http" | "https";
  hostname: string;
  port?: string;
  pathname: string;
};

const LOCAL_MEDIA_PATHNAME = "/uploads/**";
const CDN_PATHNAME = "/**";

function parseOrigin(value: string | undefined): URL | null {
  const candidate = value?.trim();
  if (!candidate) {
    return null;
  }

  try {
    return new URL(candidate);
  } catch {
    return null;
  }
}

function toRemotePattern(origin: URL, pathname: string): NextRemotePattern | null {
  if (origin.protocol !== "http:" && origin.protocol !== "https:") {
    return null;
  }

  const basePattern = {
    protocol: origin.protocol === "https:" ? "https" : "http",
    hostname: origin.hostname,
    pathname,
  } as const;

  if (origin.port) {
    return {
      ...basePattern,
      port: origin.port,
    };
  }

  return { ...basePattern };
}

export function getNetMetricImageRemotePatterns(): NextRemotePattern[] {
  const patterns: NextRemotePattern[] = [
    {
      protocol: "http",
      hostname: "localhost",
      port: "5301",
      pathname: LOCAL_MEDIA_PATHNAME,
    },
    {
      protocol: "http",
      hostname: "127.0.0.1",
      port: "5301",
      pathname: LOCAL_MEDIA_PATHNAME,
    },
    {
      protocol: "https",
      hostname: "cdn.netmetric.net",
      pathname: CDN_PATHNAME,
    },
  ];

  const dynamicOrigins = [
    process.env.ACCOUNT_MEDIA_ORIGIN,
    process.env.NEXT_PUBLIC_ACCOUNT_MEDIA_ORIGIN,
    process.env.ACCOUNT_API_BASE_URL,
    process.env.NEXT_PUBLIC_API_BASE_URL,
    process.env.MEDIA_CDN_ORIGIN,
    process.env.NEXT_PUBLIC_MEDIA_CDN_ORIGIN,
  ];

  for (const rawOrigin of dynamicOrigins) {
    const origin = parseOrigin(rawOrigin);
    if (!origin) {
      continue;
    }

    const isLikelyLocalMedia =
      origin.protocol === "http:" &&
      (origin.hostname === "localhost" || origin.hostname === "127.0.0.1");

    const pathname = isLikelyLocalMedia ? LOCAL_MEDIA_PATHNAME : CDN_PATHNAME;
    const nextPattern = toRemotePattern(origin, pathname);

    if (!nextPattern) {
      continue;
    }

    const alreadyPresent = patterns.some(
      (pattern) =>
        pattern.protocol === nextPattern.protocol &&
        pattern.hostname === nextPattern.hostname &&
        (pattern.port || "") === (nextPattern.port || "") &&
        pattern.pathname === nextPattern.pathname,
    );

    if (!alreadyPresent) {
      patterns.push(nextPattern);
    }
  }

  return patterns;
}
