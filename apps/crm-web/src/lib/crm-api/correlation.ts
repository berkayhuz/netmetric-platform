import "server-only";

export function getCorrelationIdFromHeaders(headers: Headers): string | undefined {
  return headers.get("x-request-id") ?? headers.get("x-correlation-id") ?? undefined;
}

export function applyCorrelationId(headers: Headers, correlationId: string | undefined): void {
  if (!correlationId) {
    return;
  }

  headers.set("x-request-id", correlationId);
}
