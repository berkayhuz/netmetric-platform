"use client";

import { useState, useTransition } from "react";
import type { Locale } from "@netmetric/i18n";

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
import {
  createConfirmEmailSchema,
  createResendConfirmEmailSchema,
} from "@/features/auth/schemas/confirm-email.schema";
import { getValidationText } from "@/features/auth/schemas/validation-text";
import { getAuthErrorMessage } from "@/features/auth/utils/auth-errors";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

type ConfirmEmailPanelProps = {
  locale: Locale;
  tenantId?: string;
  userId?: string;
  token?: string;
  email?: string;
};

export function ConfirmEmailPanel({
  locale,
  tenantId = "",
  userId = "",
  token = "",
  email = "",
}: ConfirmEmailPanelProps) {
  const [isPending, startTransition] = useTransition();
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const t = getTranslator(locale);
  const validation = getValidationText(locale);
  const confirmSchema = createConfirmEmailSchema(validation);
  const resendSchema = createResendConfirmEmailSchema(validation);
  const canConfirm = tenantId.length > 0 && userId.length > 0 && token.length > 0;

  function confirmEmail(): void {
    const parsed = confirmSchema.safeParse({ tenantId, userId, token });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(t("form.invalidVerificationLink"));
      return;
    }

    startTransition(async () => {
      setFormError(null);
      setSuccessMessage(null);
      try {
        await authBrowserApi.confirmEmail(parsed.data);
        const message = t("success.emailConfirmed");
        setSuccessMessage(message);
        toast.success(message, { id: "confirm-email-success" });
      } catch (error) {
        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "confirm-email-error" });
      }
    });
  }

  function resendConfirmation(event: React.FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const parsed = resendSchema.safeParse({
      tenantId,
      email: formData.get("email"),
    });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(
        tenantId.length > 0 ? t("form.enterValidEmail") : t("form.invalidVerificationLink"),
      );
      return;
    }

    startTransition(async () => {
      setFormError(null);
      setSuccessMessage(null);
      try {
        await authBrowserApi.resendConfirmEmail(parsed.data);
        const message = t("success.confirmationSent");
        setSuccessMessage(message);
        toast.success(message, { id: "resend-confirmation-success" });
      } catch (error) {
        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "resend-confirmation-error" });
      }
    });
  }

  return (
    <div className="space-y-5">
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

      {canConfirm ? (
        <Button type="button" onClick={confirmEmail} disabled={isPending} className="w-full">
          {isPending ? t("form.verifying") : t("action.confirmEmail")}
        </Button>
      ) : (
        <form onSubmit={resendConfirmation} className="space-y-4" noValidate>
          <FieldSet>
            <Field>
              <FieldLabel htmlFor="email">{t("field.email")}</FieldLabel>
              <FieldContent>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  defaultValue={email}
                  autoComplete="email"
                />
                <FieldError>{fieldErrors.email?.[0]}</FieldError>
              </FieldContent>
            </Field>
          </FieldSet>
          <Button type="submit" disabled={isPending} className="w-full">
            {isPending ? t("form.sending") : t("action.resendEmailConfirmation")}
          </Button>
        </form>
      )}
    </div>
  );
}
