import { resolveValidationMessage, type Locale } from "@netmetric/i18n";

import { getTranslator } from "../i18n/auth-i18n.shared";

export function getValidationText(locale: Locale) {
  const t = getTranslator(locale);
  const translateLike = (key: string, params?: Record<string, string | number>) =>
    t(key as never, params);

  return {
    emailRequired: resolveValidationMessage(
      translateLike,
      "required",
      t("validation.email.required"),
    ),
    emailInvalid: resolveValidationMessage(
      translateLike,
      "invalid_email",
      t("validation.email.invalid"),
    ),
    passwordRequired: resolveValidationMessage(
      translateLike,
      "required",
      t("validation.password.required"),
    ),
    passwordMin: t("validation.password.min"),
    passwordMax: t("validation.password.max"),
    confirmPasswordRequired: resolveValidationMessage(
      translateLike,
      "required",
      t("validation.confirmPassword.required"),
    ),
    passwordMatch: t("validation.password.match"),
    fullNameMin: t("validation.fullName.min"),
    fullNameMax: t("validation.fullName.max"),
    workspaceNameMin: t("validation.workspaceName.min"),
    workspaceNameMax: t("validation.workspaceName.max"),
    acceptTermsRequired: t("validation.acceptTerms.required"),
    tokenRequired: resolveValidationMessage(
      translateLike,
      "required",
      t("validation.token.required"),
    ),
    userIdRequired: resolveValidationMessage(
      translateLike,
      "required",
      t("validation.userId.required"),
    ),
    confirmTokenRequired: resolveValidationMessage(
      translateLike,
      "required",
      t("validation.confirmToken.required"),
    ),
    codeMin: t("validation.code.min"),
    codeMax: t("validation.code.max"),
    recoveryCodeMin: t("validation.recoveryCode.min"),
    recoveryCodeMax: t("validation.recoveryCode.max"),
    workspaceRequired: resolveValidationMessage(
      translateLike,
      "required",
      t("validation.workspace.required"),
    ),
  };
}

export type ValidationText = ReturnType<typeof getValidationText>;
