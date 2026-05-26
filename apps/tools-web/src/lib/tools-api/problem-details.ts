import "server-only";

export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  errorCode?: string;
};

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function normalizeErrors(value: unknown): Record<string, string[]> | undefined {
  if (!isRecord(value)) {
    return undefined;
  }

  const entries = Object.entries(value)
    .map(([key, item]) => {
      if (!Array.isArray(item)) {
        return null;
      }

      const messages = item.filter((message) => typeof message === "string");
      return messages.length > 0 ? [key, messages] : null;
    })
    .filter((entry): entry is [string, string[]] => Boolean(entry));

  return entries.length > 0 ? Object.fromEntries(entries) : undefined;
}

export function normalizeProblemDetails(payload: unknown): ProblemDetails | undefined {
  if (!isRecord(payload)) {
    return undefined;
  }

  const output: ProblemDetails = {};

  if (typeof payload.type === "string") output.type = payload.type;
  if (typeof payload.title === "string") output.title = payload.title;
  if (typeof payload.status === "number") output.status = payload.status;
  if (typeof payload.detail === "string") output.detail = payload.detail;
  if (typeof payload.instance === "string") output.instance = payload.instance;
  if (typeof payload.errorCode === "string") output.errorCode = payload.errorCode;

  const errors = normalizeErrors(payload.errors);
  if (errors) output.errors = errors;

  return Object.keys(output).length > 0 ? output : undefined;
}
