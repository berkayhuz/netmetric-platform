export type ValidationMessageCode =
  | "required"
  | "invalid_email"
  | "min_length"
  | "max_length"
  | "invalid_date"
  | "unknown";

export type ActionErrorCode =
  | "validation"
  | "conflict"
  | "unauthorized"
  | "forbidden"
  | "not_found"
  | "rate_limited"
  | "server_error"
  | "unknown";

type TranslateLike = (key: string, params?: Record<string, string | number>) => string;

const validationMessageKeyByCode: Record<ValidationMessageCode, string> = {
  required: "common.validation.required",
  invalid_email: "common.validation.invalidEmail",
  min_length: "common.validation.minLength",
  max_length: "common.validation.maxLength",
  invalid_date: "common.validation.invalidDate",
  unknown: "common.validation.unknown",
};

const actionErrorMessageKeyByCode: Record<ActionErrorCode, string> = {
  validation: "common.action.errors.validation",
  conflict: "common.action.errors.conflict",
  unauthorized: "common.action.errors.unauthorized",
  forbidden: "common.action.errors.forbidden",
  not_found: "common.action.errors.notFound",
  rate_limited: "common.action.errors.rateLimited",
  server_error: "common.action.errors.serverError",
  unknown: "common.action.errors.unknown",
};

function resolveKey(
  t: TranslateLike,
  key: string,
  fallback: string,
  params?: Record<string, string | number>,
): string {
  const translated = t(key, params);
  return translated === key ? fallback : translated;
}

export function resolveValidationMessage(
  t: TranslateLike,
  code: ValidationMessageCode,
  fallback: string,
  params?: Record<string, string | number>,
): string {
  return resolveKey(t, validationMessageKeyByCode[code], fallback, params);
}

export function resolveActionErrorMessage(
  t: TranslateLike,
  code: ActionErrorCode,
  fallback: string,
  params?: Record<string, string | number>,
): string {
  return resolveKey(t, actionErrorMessageKeyByCode[code], fallback, params);
}
