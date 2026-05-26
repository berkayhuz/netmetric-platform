"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";

import {
  Alert,
  AlertDescription,
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FieldSet,
  Input,
} from "@netmetric/ui";
import { toast } from "@netmetric/ui/client";

import { authBrowserApi } from "@/features/auth/api/auth-browser-api";
import { authRoutes } from "@/features/auth/config/auth-routes";
import { getTranslator } from "@/features/auth/i18n/auth-i18n.client";
import type { Locale } from "@/features/auth/i18n/auth-i18n.shared";
import {
  createResetPasswordSchema,
  type ResetPasswordInput,
} from "@/features/auth/schemas/reset-password.schema";
import { getValidationText } from "@/features/auth/schemas/validation-text";
import { getAuthErrorMessage } from "@/features/auth/utils/auth-errors";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

type ResetPasswordFormProps = {
  locale: Locale;
  tenantId?: string;
  userId?: string;
  token?: string;
};

export function ResetPasswordForm({
  locale,
  tenantId = "",
  userId = "",
  token = "",
}: ResetPasswordFormProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const t = getTranslator(locale);
  const schema = createResetPasswordSchema(getValidationText(locale));
  const hasResetContext = tenantId.length > 0 && userId.length > 0 && token.length > 0;

  function submitResetPassword(input: ResetPasswordInput): void {
    startTransition(async () => {
      setFormError(null);
      setFieldErrors({});

      try {
        await authBrowserApi.resetPassword(input);
        toast.success(t("success.passwordReset"), { id: "reset-password-success" });
        router.replace(`${authRoutes.login}?passwordReset=success`);
      } catch (error) {
        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "reset-password-error" });
      }
    });
  }

  function onSubmit(event: React.FormEvent<HTMLFormElement>): void {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    const parsed = schema.safeParse({
      tenantId: formData.get("tenantId"),
      userId: formData.get("userId"),
      token: formData.get("token"),
      password: formData.get("password"),
      confirmPassword: formData.get("confirmPassword"),
    });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(hasResetContext ? t("form.fixErrors") : t("form.invalidVerificationLink"));
      return;
    }

    submitResetPassword(parsed.data);
  }

  return (
    <form onSubmit={onSubmit} className="space-y-5" noValidate>
      {formError ? (
        <Alert variant="destructive">
          <AlertDescription>{formError}</AlertDescription>
        </Alert>
      ) : null}

      <input type="hidden" name="tenantId" value={tenantId} />
      <input type="hidden" name="userId" value={userId} />
      <input type="hidden" name="token" value={token} />

      <FieldSet>
        <Field>
          <FieldLabel htmlFor="password">{t("field.newPassword")}</FieldLabel>
          <FieldContent>
            <Input id="password" name="password" type="password" autoComplete="new-password" />
            <FieldError>{fieldErrors.password?.[0]}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="confirmPassword">{t("field.newPasswordAgain")}</FieldLabel>
          <FieldContent>
            <Input
              id="confirmPassword"
              name="confirmPassword"
              type="password"
              autoComplete="new-password"
            />
            <FieldError>{fieldErrors.confirmPassword?.[0]}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? t("form.updating") : t("action.updatePassword")}
      </Button>
    </form>
  );
}
