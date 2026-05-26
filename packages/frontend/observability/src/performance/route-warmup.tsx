"use client";

import { useEffect, useMemo, useRef } from "react";

import { getWarmupHrefs, type CriticalRouteWarmupRoute } from "./route-warmup-core";

type Prefetch = (href: string) => void | Promise<void>;

export type CriticalRouteWarmupProps = Readonly<{
  app: string;
  routes: readonly CriticalRouteWarmupRoute[];
  currentPath: string | null | undefined;
  prefetch: Prefetch;
  enabled?: boolean;
  maxRoutes?: number;
  initialDelayMs?: number;
  staggerMs?: number;
  idleTimeoutMs?: number;
  onPrefetchError?: (href: string, error: unknown) => void;
}>;

type IdleDeadline = Readonly<{
  didTimeout: boolean;
  timeRemaining(): number;
}>;

type WindowWithIdleCallback = Window &
  typeof globalThis & {
    requestIdleCallback?: (
      callback: (deadline: IdleDeadline) => void,
      options?: { timeout?: number },
    ) => number;
    cancelIdleCallback?: (handle: number) => void;
  };

export function CriticalRouteWarmup({
  routes,
  currentPath,
  prefetch,
  enabled = true,
  maxRoutes = 4,
  initialDelayMs = 900,
  staggerMs = 180,
  idleTimeoutMs = 2500,
  onPrefetchError,
}: CriticalRouteWarmupProps) {
  const hrefs = useMemo(
    () => getWarmupHrefs(routes, currentPath, maxRoutes),
    [currentPath, maxRoutes, routes],
  );
  const prefetchRef = useRef(prefetch);
  const onPrefetchErrorRef = useRef(onPrefetchError);

  useEffect(() => {
    prefetchRef.current = prefetch;
  }, [prefetch]);

  useEffect(() => {
    onPrefetchErrorRef.current = onPrefetchError;
  }, [onPrefetchError]);

  useEffect(() => {
    if (!enabled || hrefs.length === 0 || typeof window === "undefined") {
      return undefined;
    }

    const pendingTimeouts = new Set<number>();
    const idleWindow = window as WindowWithIdleCallback;
    let idleHandle: number | null = null;

    const runWarmup = () => {
      hrefs.forEach((href, index) => {
        const timeoutHandle = window.setTimeout(() => {
          pendingTimeouts.delete(timeoutHandle);
          try {
            void Promise.resolve(prefetchRef.current(href)).catch((error: unknown) => {
              onPrefetchErrorRef.current?.(href, error);
            });
          } catch (error) {
            onPrefetchErrorRef.current?.(href, error);
          }
        }, index * staggerMs);

        pendingTimeouts.add(timeoutHandle);
      });
    };

    const startHandle = window.setTimeout(() => {
      pendingTimeouts.delete(startHandle);

      if (typeof idleWindow.requestIdleCallback === "function") {
        idleHandle = idleWindow.requestIdleCallback(runWarmup, { timeout: idleTimeoutMs });
        return;
      }

      const fallbackHandle = window.setTimeout(runWarmup, idleTimeoutMs);
      pendingTimeouts.add(fallbackHandle);
    }, initialDelayMs);

    pendingTimeouts.add(startHandle);

    return () => {
      for (const timeoutHandle of pendingTimeouts) {
        window.clearTimeout(timeoutHandle);
      }

      if (idleHandle !== null && typeof idleWindow.cancelIdleCallback === "function") {
        idleWindow.cancelIdleCallback(idleHandle);
      }
    };
  }, [enabled, hrefs, idleTimeoutMs, initialDelayMs, staggerMs]);

  return null;
}
