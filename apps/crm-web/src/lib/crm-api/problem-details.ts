import "server-only";

import type { ProblemDetails } from "./crm-api-types";

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function readString(value: unknown): string | undefined {
  return typeof value === "string" && value.length > 0 ? value : undefined;
}

function readNumber(value: unknown): number | undefined {
  return typeof value === "number" ? value : undefined;
}

function readErrors(value: unknown): Record<string, string[]> | undefined {
  if (!isRecord(value)) {
    return undefined;
  }

  const entries = Object.entries(value).flatMap(([key, fieldValue]) => {
    if (!Array.isArray(fieldValue)) {
      return [];
    }

    const messages = fieldValue.filter((item): item is string => typeof item === "string");
    return messages.length > 0 ? [[key, messages] as const] : [];
  });

  return entries.length > 0 ? Object.fromEntries(entries) : undefined;
}

export function normalizeProblemDetails(payload: unknown): ProblemDetails | undefined {
  if (!isRecord(payload)) {
    return undefined;
  }

  const result: ProblemDetails = {};
  const type = readString(payload.type);
  const title = readString(payload.title);
  const status = readNumber(payload.status);
  const detail = readString(payload.detail);
  const instance = readString(payload.instance);
  const traceId = readString(payload.traceId);
  const correlationId = readString(payload.correlationId);
  const errorCode = readString(payload.errorCode);
  const errors = readErrors(payload.errors);

  if (type) result.type = type;
  if (title) result.title = title;
  if (status !== undefined) result.status = status;
  if (detail) result.detail = detail;
  if (instance) result.instance = instance;
  if (traceId) result.traceId = traceId;
  if (correlationId) result.correlationId = correlationId;
  if (errorCode) result.errorCode = errorCode;
  if (errors) result.errors = errors;

  if (
    result.type ||
    result.title ||
    result.status !== undefined ||
    result.detail ||
    result.instance ||
    result.errorCode ||
    result.errors
  ) {
    return result;
  }

  return undefined;
}
