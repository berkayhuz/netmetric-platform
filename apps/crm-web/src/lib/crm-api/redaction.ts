import "server-only";

const sensitiveValuePattern =
  /((?:bearer\s+)?[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+|__Secure-[^;\s]+=[^;\s]+|__Host-[^;\s]+=[^;\s]+)/gi;

const sensitiveKeyPattern =
  /(authorization|cookie|password|token|secret|verification|recovery|session)/i;

export function sanitizeText(value: string | undefined): string | undefined {
  if (!value) {
    return undefined;
  }

  return value.replace(sensitiveValuePattern, "[redacted]");
}

export function sanitizeFieldErrors(
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
