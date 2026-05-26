"use client";

import { useEffect } from "react";

const SENSITIVE_KEY_PATTERN =
  /(password|passcode|token|secret|authorization|cookie|email|phone|ssn|credit|card|ip)/i;

export type MonitoringEnvironment = "development" | "test" | "staging" | "production";
export type MonitoringProviderKind = "noop" | "console";

export type ErrorMonitoringConfig = {
  app: "auth-web" | "account-web" | "crm-web" | "tools-web" | "public-web";
  environment: MonitoringEnvironment;
  provider: MonitoringProviderKind;
  enabled: boolean;
};

export type MonitoringEvent = {
  name: string;
  message: string;
  stack?: string;
  tags: Record<string, string>;
  extras?: Record<string, unknown>;
};

export interface ErrorMonitoringProvider {
  capture(event: MonitoringEvent): void;
}

export function createMonitoringConfig(input: {
  app: ErrorMonitoringConfig["app"];
  environment?: string | undefined;
  provider?: string | undefined;
}): ErrorMonitoringConfig {
  const environment = normalizeEnvironment(input.environment);
  const provider = normalizeProvider(input.provider);
  const enabled = environment === "production" || environment === "staging";

  if (enabled && provider === "noop" && typeof console !== "undefined") {
    console.warn(`[monitoring] ${input.app}: provider is noop in ${environment}.`);
  }

  return {
    app: input.app,
    environment,
    provider: enabled ? provider : "noop",
    enabled,
  };
}

export function createProvider(config: ErrorMonitoringConfig): ErrorMonitoringProvider {
  if (config.provider === "console") {
    return {
      capture(event) {
        console.error("[monitoring-event]", redactEvent(event));
      },
    };
  }

  return { capture() {} };
}

export function redactEvent(event: MonitoringEvent): MonitoringEvent {
  const next: MonitoringEvent = {
    ...event,
    message: redactText(event.message),
    tags: Object.fromEntries(Object.entries(event.tags).map(([k, v]) => [k, redactTag(k, v)])),
  };

  if (event.stack) {
    next.stack = redactText(event.stack);
  }

  if (event.extras) {
    next.extras = redactObject(event.extras);
  }

  return next;
}

export function ErrorMonitoringBootstrap({ config }: { config: ErrorMonitoringConfig }) {
  useEffect(() => {
    const provider = createProvider(config);
    const baseTags = { app: config.app, environment: config.environment };

    const onError = (event: ErrorEvent) => {
      const errorEvent: MonitoringEvent = {
        name: "window.error",
        message: event.message || "Unhandled client error",
        tags: baseTags,
        extras: {
          filename: event.filename,
          lineno: event.lineno,
          colno: event.colno,
        },
      };

      if (event.error instanceof Error && event.error.stack) {
        errorEvent.stack = event.error.stack;
      }

      provider.capture(errorEvent);
    };

    const onUnhandledRejection = (event: PromiseRejectionEvent) => {
      const reason = event.reason;
      const errorEvent: MonitoringEvent = {
        name: "window.unhandledrejection",
        message: reason instanceof Error ? reason.message : String(reason ?? "Unhandled rejection"),
        tags: baseTags,
      };

      if (reason instanceof Error && reason.stack) {
        errorEvent.stack = reason.stack;
      }

      provider.capture(errorEvent);
    };

    window.addEventListener("error", onError);
    window.addEventListener("unhandledrejection", onUnhandledRejection);
    return () => {
      window.removeEventListener("error", onError);
      window.removeEventListener("unhandledrejection", onUnhandledRejection);
    };
  }, [config]);

  return null;
}

function normalizeEnvironment(value?: string): MonitoringEnvironment {
  switch ((value ?? "").toLowerCase()) {
    case "production":
      return "production";
    case "staging":
      return "staging";
    case "test":
      return "test";
    default:
      return "development";
  }
}

function normalizeProvider(value?: string): MonitoringProviderKind {
  return (value ?? "").toLowerCase() === "console" ? "console" : "noop";
}

function redactText(value: string): string {
  return value.replace(/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/gi, "[redacted-email]");
}

function redactTag(key: string, value: string): string {
  if (SENSITIVE_KEY_PATTERN.test(key)) {
    return "[redacted]";
  }

  return redactText(value).slice(0, 200);
}

function redactObject(value: Record<string, unknown>): Record<string, unknown> {
  const result: Record<string, unknown> = {};
  for (const [key, entry] of Object.entries(value)) {
    if (SENSITIVE_KEY_PATTERN.test(key)) {
      result[key] = "[redacted]";
      continue;
    }

    result[key] = typeof entry === "string" ? redactText(entry).slice(0, 1000) : entry;
  }

  return result;
}
