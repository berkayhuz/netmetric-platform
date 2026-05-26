import "server-only";

import type { ProblemDetails } from "./problem-details";
import { sanitizeFieldErrors, sanitizeText } from "./redaction";

export type ToolsApiErrorKind =
  | "unauthorized"
  | "forbidden"
  | "not_found"
  | "conflict"
  | "payload_too_large"
  | "unsupported_media_type"
  | "validation"
  | "rate_limited"
  | "server_error"
  | "upstream_unavailable"
  | "unknown";

function sanitizeProblem(problem: ProblemDetails | undefined): ProblemDetails | undefined {
  if (!problem) {
    return undefined;
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

export function statusToToolsApiErrorKind(status: number): ToolsApiErrorKind {
  if (status === 400 || status === 422) return "validation";
  if (status === 401) return "unauthorized";
  if (status === 403) return "forbidden";
  if (status === 404) return "not_found";
  if (status === 409) return "conflict";
  if (status === 413) return "payload_too_large";
  if (status === 415) return "unsupported_media_type";
  if (status === 429) return "rate_limited";
  if (status >= 500) return "server_error";
  return "unknown";
}

export class ToolsApiError extends Error {
  public readonly status: number;
  public readonly kind: ToolsApiErrorKind;
  public readonly problem: ProblemDetails | undefined;
  public readonly correlationId: string | undefined;

  public constructor(params: {
    message: string;
    status: number;
    kind?: ToolsApiErrorKind;
    problem?: ProblemDetails;
    correlationId?: string;
  }) {
    super(sanitizeText(params.message) ?? "Tools API request failed.");
    this.name = "ToolsApiError";
    this.status = params.status;
    this.kind = params.kind ?? statusToToolsApiErrorKind(params.status);
    this.problem = sanitizeProblem(params.problem);
    this.correlationId = sanitizeText(params.correlationId);
  }
}
