"use client";

export {
  createCriticalRouteWarmupConfig,
  getWarmupHrefs,
  isWarmupSafeHref,
  type CriticalRouteWarmupRoute,
} from "./route-warmup-core";
export { CriticalRouteWarmup, type CriticalRouteWarmupProps } from "./route-warmup";
export {
  ClientRoutePerformanceReporter,
  type ClientRoutePerformanceReporterProps,
} from "./client-route-metrics";
