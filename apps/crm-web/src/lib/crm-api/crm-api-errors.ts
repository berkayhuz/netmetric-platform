import "server-only";

import type { ProblemDetails } from "./crm-api-types";
import { sanitizeFieldErrors, sanitizeText } from "./redaction";

export type CrmApiErrorKind =
  | "unauthorized"
  | "forbidden"
  | "not_found"
  | "validation"
  | "conflict"
  | "rate_limited"
  | "server_error"
  | "upstream_unavailable"
  | "unknown";

function sanitizeProblem(
  problem: ProblemDetails | undefined,
  status: number,
): ProblemDetails | undefined {
  if (!problem) {
    return undefined;
  }

  if (status >= 500) {
    return {
      title: "CRM service unavailable.",
      detail: "Please try again later.",
    };
  }

  const output: ProblemDetails = { ...problem };

  const title = sanitizeText(problem.title);
  const detail = sanitizeText(problem.detail);
  const errorCode = sanitizeText(problem.errorCode);
  const errors = sanitizeFieldErrors(problem.errors);

  if (title) output.title = title;
  else delete output.title;

  if (detail) output.detail = detail;
  else delete output.detail;

  if (errorCode) output.errorCode = errorCode;
  else delete output.errorCode;

  if (errors) output.errors = errors;
  else delete output.errors;

  return output;
}

export function statusToCrmApiErrorKind(status: number): CrmApiErrorKind {
  if (status === 401) return "unauthorized";
  if (status === 403) return "forbidden";
  if (status === 404) return "not_found";
  if (status === 409) return "conflict";
  if (status === 400 || status === 422) return "validation";
  if (status === 429) return "rate_limited";
  if (status >= 500) return "server_error";
  return "unknown";
}

export class CrmApiError extends Error {
  public readonly status: number;
  public readonly kind: CrmApiErrorKind;
  public readonly problem: ProblemDetails | undefined;
  public readonly correlationId: string | undefined;

  public constructor(params: {
    message: string;
    status: number;
    kind?: CrmApiErrorKind;
    problem?: ProblemDetails;
    correlationId?: string;
  }) {
    super(sanitizeText(params.message) ?? "CRM API request failed.");
    this.name = "CrmApiError";
    this.status = params.status;
    this.kind = params.kind ?? statusToCrmApiErrorKind(params.status);
    this.problem = sanitizeProblem(params.problem, params.status);
    this.correlationId = sanitizeText(params.correlationId);
  }
}
