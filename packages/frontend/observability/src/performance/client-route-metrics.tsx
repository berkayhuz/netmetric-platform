"use client";

import { useEffect } from "react";

export type ClientRoutePerformanceReporterProps = Readonly<{
  app: string;
  route: string;
  enabled?: boolean;
}>;

export function ClientRoutePerformanceReporter({
  app,
  route,
  enabled = false,
}: ClientRoutePerformanceReporterProps) {
  useEffect(() => {
    if (!enabled || typeof performance === "undefined") {
      return;
    }

    const markName = `netmetric:${app}:${route}:hydrated`;
    performance.mark(markName);

    const navigation = performance.getEntriesByType("navigation")[0] as
      | PerformanceNavigationTiming
      | undefined;

    if (typeof console === "undefined" || !navigation) {
      return;
    }

    console.info("[netmetric-route-performance]", {
      app,
      route,
      ttfbMs: round(navigation.responseStart - navigation.requestStart),
      transferMs: round(navigation.responseEnd - navigation.responseStart),
      domInteractiveMs: round(navigation.domInteractive - navigation.startTime),
      hydratedMs: round(performance.now() - navigation.startTime),
    });
  }, [app, enabled, route]);

  return null;
}

function round(value: number): number {
  return Math.max(0, Math.round(value * 10) / 10);
}
