"use client";

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
import { getTranslator } from "@/features/auth/i18n/auth-i18n.client";
import type { Locale } from "@/features/auth/i18n/auth-i18n.shared";
import {
  createForgotPasswordSchema,
  type ForgotPasswordInput,
} from "@/features/auth/schemas/forgot-password.schema";
import { getValidationText } from "@/features/auth/schemas/validation-text";
import { getAuthErrorMessage } from "@/features/auth/utils/auth-errors";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

type ForgotPasswordFormProps = { locale: Locale };

export function ForgotPasswordForm({ locale }: ForgotPasswordFormProps) {
  const [isPending, startTransition] = useTransition();
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const t = getTranslator(locale);
  const schema = createForgotPasswordSchema(getValidationText(locale));

  function submitForgotPassword(input: ForgotPasswordInput): void {
    startTransition(async () => {
      setSuccessMessage(null);
      setFormError(null);
      setFieldErrors({});

      try {
        await authBrowserApi.forgotPassword(input);
        const message = t("success.forgotPasswordSent");
        setSuccessMessage(message);
        toast.success(message, { id: "forgot-password-success" });
      } catch (error) {
        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "forgot-password-error" });
      }
    });
  }

  function onSubmit(event: React.FormEvent<HTMLFormElement>): void {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    const parsed = schema.safeParse({ email: formData.get("email") });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(t("form.fixErrors"));
      return;
    }

    submitForgotPassword(parsed.data);
  }

  return (
    <form onSubmit={onSubmit} className="space-y-5" noValidate>
      {formError ? (
        <Alert variant="destructive">
          <AlertDescription>{formError}</AlertDescription>
        </Alert>
      ) : null}
      {successMessage ? (
        <Alert>
          <AlertDescription>{successMessage}</AlertDescription>
        </Alert>
      ) : null}

      <FieldSet>
        <Field>
          <FieldLabel htmlFor="email">{t("field.email")}</FieldLabel>
          <FieldContent>
            <Input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              aria-invalid={Boolean(fieldErrors.email?.[0])}
            />
            <FieldError>{fieldErrors.email?.[0]}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? t("form.sending") : t("action.sendResetLink")}
      </Button>
    </form>
  );
}
