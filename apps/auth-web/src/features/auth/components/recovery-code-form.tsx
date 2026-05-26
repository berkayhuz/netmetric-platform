"use client";

import Link from "next/link";
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
  createRecoveryCodeSchema,
  type RecoveryCodeInput,
} from "@/features/auth/schemas/recovery-code.schema";
import { getValidationText } from "@/features/auth/schemas/validation-text";
import { getAuthErrorMessage } from "@/features/auth/utils/auth-errors";
import { getRedirectAfterAuth } from "@/features/auth/utils/redirect-after-auth";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

type RecoveryCodeFormProps = {
  locale: Locale;
  identifier?: string;
  challengeId?: string;
  returnUrl?: string;
};

export function RecoveryCodeForm({
  locale,
  identifier = "",
  challengeId = "",
  returnUrl = "",
}: RecoveryCodeFormProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const t = getTranslator(locale);
  const schema = createRecoveryCodeSchema(getValidationText(locale));

  function submitRecoveryCode(input: RecoveryCodeInput): void {
    startTransition(async () => {
      setFormError(null);
      setFieldErrors({});

      try {
        const result = await authBrowserApi.verifyRecoveryCode(input);
        if ("mfaRequired" in result && result.mfaRequired) {
          const message = t("auth.error.recovery_invalid");
          setFormError(message);
          toast.error(message, { id: "recovery-code-error" });
          return;
        }

        const redirectUrl = "redirectUrl" in result ? result.redirectUrl : undefined;
        toast.success(t("success.recoveryCodeVerified"), { id: "recovery-code-success" });
        router.replace(redirectUrl ?? getRedirectAfterAuth(input.returnUrl));
      } catch (error) {
        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "recovery-code-error" });
      }
    });
  }

  function onSubmit(event: React.FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const parsed = schema.safeParse({
      identifier: formData.get("identifier"),
      password: formData.get("password"),
      recoveryCode: formData.get("recoveryCode"),
      challengeId: formData.get("challengeId"),
      returnUrl,
    });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(t("form.fixErrors"));
      return;
    }

    submitRecoveryCode(parsed.data);
  }

  const mfaHref = `${authRoutes.mfa}?${new URLSearchParams({ identifier, challengeId, returnUrl }).toString()}`;

  return (
    <form onSubmit={onSubmit} className="space-y-5" noValidate>
      {formError ? (
        <Alert variant="destructive">
          <AlertDescription>{formError}</AlertDescription>
        </Alert>
      ) : null}

      <input type="hidden" name="challengeId" value={challengeId} />

      <FieldSet>
        <Field>
          <FieldLabel htmlFor="identifier">{t("field.email")}</FieldLabel>
          <FieldContent>
            <Input
              id="identifier"
              name="identifier"
              type="text"
              defaultValue={identifier}
              autoComplete="username"
            />
            <FieldError>{fieldErrors.identifier?.[0]}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="password">{t("field.password")}</FieldLabel>
          <FieldContent>
            <Input id="password" name="password" type="password" autoComplete="current-password" />
            <FieldError>{fieldErrors.password?.[0]}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="recoveryCode">{t("field.recoveryCode")}</FieldLabel>
          <FieldContent>
            <Input id="recoveryCode" name="recoveryCode" type="text" autoComplete="one-time-code" />
            <FieldError>{fieldErrors.recoveryCode?.[0]}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? t("form.verifying") : t("action.useRecoveryCode")}
      </Button>

      <p className="text-center text-sm text-muted-foreground">
        {t("hint.hasMfaCode")}{" "}
        <Link
          href={mfaHref}
          className="font-medium text-foreground underline-offset-4 hover:underline"
        >
          {t("link.useMfaCode")}
        </Link>
      </p>
    </form>
  );
}
