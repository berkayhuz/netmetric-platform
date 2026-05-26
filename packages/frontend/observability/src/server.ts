export type ServerPerformanceEvent = Readonly<{
  app: string;
  component: string;
  name: string;
  durationMs: number;
  tags: Record<string, string>;
}>;

export type ServerPerformanceLogger = Readonly<{
  measure<T>(name: string, operation: () => Promise<T>, tags?: Record<string, string>): Promise<T>;
  record(name: string, durationMs: number, tags?: Record<string, string>): void;
}>;

export function createServerPerformanceLogger(input: {
  app: string;
  component: string;
  enabled?: boolean;
  sink?: (event: ServerPerformanceEvent) => void;
}): ServerPerformanceLogger {
  const enabled = input.enabled ?? false;
  const sink = input.sink ?? defaultSink;

  return {
    async measure<T>(
      name: string,
      operation: () => Promise<T>,
      tags: Record<string, string> = {},
    ): Promise<T> {
      const startedAt = performance.now();

      try {
        return await operation();
      } finally {
        record(name, performance.now() - startedAt, tags);
      }
    },
    record,
  };

  function record(name: string, durationMs: number, tags: Record<string, string> = {}): void {
    if (!enabled) {
      return;
    }

    sink({
      app: input.app,
      component: input.component,
      name,
      durationMs: Math.max(0, Math.round(durationMs * 10) / 10),
      tags: redactTags(tags),
    });
  }
}

function defaultSink(event: ServerPerformanceEvent): void {
  if (typeof console !== "undefined") {
    console.info("[netmetric-server-performance]", event);
  }
}

function redactTags(tags: Record<string, string>): Record<string, string> {
  return Object.fromEntries(
    Object.entries(tags).map(([key, value]) => [
      key,
      /token|secret|cookie|authorization|email|phone/i.test(key)
        ? "[redacted]"
        : value.slice(0, 200),
    ]),
  );
}
