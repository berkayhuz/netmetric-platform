import "server-only";

import type { ProblemDetails } from "./problem-details";

export type AccountApiErrorKind =
  | "unauthorized"
  | "forbidden"
  | "not_found"
  | "conflict"
  | "validation"
  | "rate_limited"
  | "server_error"
  | "upstream_unavailable"
  | "unknown";

const sensitiveValuePattern =
  /((?:bearer\s+)?[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+|__Secure-[^;\s]+=[^;\s]+|__Host-[^;\s]+=[^;\s]+)/gi;

const sensitiveKeyPattern =
  /(authorization|cookie|password|token|secret|verification|recovery|session)/i;

function sanitizeText(value: string | undefined): string | undefined {
  if (!value) {
    return undefined;
  }

  return value.replace(sensitiveValuePattern, "[redacted]");
}

function sanitizeFieldErrors(
  errors: Record<string, string[]> | undefined,
): Record<string, string[]> | undefined {
  if (!errors) {
    return undefined;
  }

  return Object.fromEntries(
    Object.entries(errors).map(([field, messages]) => {
      const safeField = sensitiveKeyPattern.test(field) ? "[redacted]" : field;
      const safeMessages = messages.map((message) => sanitizeText(message) ?? "Invalid value.");
      return [safeField, safeMessages];
    }),
  );
}

function sanitizeProblem(problem: ProblemDetails | undefined): ProblemDetails | undefined {
  if (!problem) {
    return undefined;
  }

  const result: ProblemDetails = { ...problem };
  const detail = sanitizeText(problem.detail);
  const title = sanitizeText(problem.title);
  const errorCode = sanitizeText(problem.errorCode);
  const errors = sanitizeFieldErrors(problem.errors);

  if (detail) result.detail = detail;
  else delete result.detail;

  if (title) result.title = title;
  else delete result.title;

  if (errorCode) result.errorCode = errorCode;
  else delete result.errorCode;

  if (errors) result.errors = errors;
  else delete result.errors;

  return result;
}

export function statusToAccountApiErrorKind(status: number): AccountApiErrorKind {
  if (status === 401) return "unauthorized";
  if (status === 403) return "forbidden";
  if (status === 404) return "not_found";
  if (status === 409) return "conflict";
  if (status === 422) return "validation";
  if (status === 429) return "rate_limited";
  if (status >= 500) return "server_error";
  return "unknown";
}

export class AccountApiError extends Error {
  public readonly status: number;
  public readonly kind: AccountApiErrorKind;
  public readonly problem: ProblemDetails | undefined;
  public readonly correlationId: string | undefined;

  public constructor(params: {
    message: string;
    status: number;
    kind?: AccountApiErrorKind;
    problem?: ProblemDetails;
    correlationId?: string;
  }) {
    super(sanitizeText(params.message) ?? "Account API request failed.");
    this.name = "AccountApiError";
    this.status = params.status;
    this.kind = params.kind ?? statusToAccountApiErrorKind(params.status);
    this.problem = sanitizeProblem(params.problem);
    this.correlationId = sanitizeText(params.correlationId);
  }
}
