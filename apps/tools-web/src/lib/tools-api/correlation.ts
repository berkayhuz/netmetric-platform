import "server-only";

export function applyCorrelationId(headers: Headers, correlationId: string | undefined): void {
  if (!correlationId) {
    return;
  }

  headers.set("x-correlation-id", correlationId);
}

export function getCorrelationIdFromHeaders(headers: Headers): string | undefined {
  return headers.get("x-correlation-id") ?? headers.get("x-request-id") ?? undefined;
}
