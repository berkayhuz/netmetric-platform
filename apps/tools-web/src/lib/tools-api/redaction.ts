import "server-only";

const sensitiveValuePattern =
  /((?:bearer\s+)?[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+|__Secure-[^;\s]+=[^;\s]+|__Host-[^;\s]+=[^;\s]+)/gi;

const sensitiveKeyPattern =
  /(authorization|cookie|password|token|secret|verification|recovery|session|mfa)/i;

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
    Object.entries(errors).map(([key, values]) => {
      const safeKey = sensitiveKeyPattern.test(key) ? "[redacted]" : key;
      const safeValues = values.map((value) => sanitizeText(value) ?? "Invalid value.");
      return [safeKey, safeValues];
    }),
  );
}
