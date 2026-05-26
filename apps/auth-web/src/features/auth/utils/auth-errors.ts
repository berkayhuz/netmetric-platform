import type { MessageKey } from "@netmetric/i18n";

import { ApiError } from "@/lib/api/api-error";

import { getClientLocale, getTranslator } from "../i18n/auth-i18n.client";
import type { Locale } from "../i18n/auth-i18n.shared";

const authErrorCodeKeyAliases: Record<string, MessageKey> = {
  email_confirmation_required: "auth.error.email_not_confirmed",
  invalid_email_confirmation_token: "auth.error.invalid_token",
  invalid_password_reset_token: "auth.error.invalid_token",
  invalid_mfa_code: "auth.error.mfa_invalid",
  invalid_recovery_code: "auth.error.recovery_invalid",
  tenant_resolution_required: "auth.error.tenant_required",
};

export function getAuthErrorMessage(error: unknown, locale?: Locale): string {
  const t = getTranslator(getClientLocale(locale));

  if (error instanceof ApiError) {
    const errorCode = error.problem?.errorCode;

    if (errorCode) {
      const aliasedKey = authErrorCodeKeyAliases[errorCode];

      if (aliasedKey) {
        return t(aliasedKey);
      }

      const key = `auth.error.${errorCode}` as MessageKey;
      const translated = t(key);

      if (translated !== key) {
        return translated;
      }
    }

    const validationErrorMessage = Object.values(error.problem?.errors ?? {})[0]?.[0];
    if (validationErrorMessage) {
      return validationErrorMessage;
    }

    if (error.problem?.detail) {
      return error.problem.detail;
    }
  }

  return t("auth.error.default");
}
