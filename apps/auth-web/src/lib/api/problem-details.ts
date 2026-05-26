export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  correlationId?: string;
  errorCode?: string;
  errors?: Record<string, string[]>;
};

export function isProblemDetails(value: unknown): value is ProblemDetails {
  if (!value || typeof value !== "object") {
    return false;
  }

  return "title" in value || "detail" in value || "status" in value;
}
