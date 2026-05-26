"use client";

import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition } from "react";

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
import { InputOTP, InputOTPGroup, InputOTPSlot, toast } from "@netmetric/ui/client";

import { authBrowserApi } from "@/features/auth/api/auth-browser-api";
import { getTranslator } from "@/features/auth/i18n/auth-i18n.client";
import type { Locale } from "@/features/auth/i18n/auth-i18n.shared";
import { createMfaSchema, type MfaInput } from "@/features/auth/schemas/mfa.schema";
import { getValidationText } from "@/features/auth/schemas/validation-text";
import { getAuthErrorMessage } from "@/features/auth/utils/auth-errors";
import { getRedirectAfterAuth } from "@/features/auth/utils/redirect-after-auth";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

type MfaFormProps = {
  locale: Locale;
  identifier?: string;
  challengeId?: string;
  returnUrl?: string;
};

export function MfaForm({
  locale,
  identifier = "",
  challengeId = "",
  returnUrl = "",
}: MfaFormProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [code, setCode] = useState("");

  const t = useMemo(() => getTranslator(locale), [locale]);
  const schema = useMemo(() => createMfaSchema(getValidationText(locale)), [locale]);

  function submitMfa(input: MfaInput): void {
    startTransition(async () => {
      setFormError(null);
      setFieldErrors({});

      try {
        const result = await authBrowserApi.verifyMfa(input);
        if ("mfaRequired" in result && result.mfaRequired) {
          const message = t("auth.error.mfa_invalid");
          setFormError(message);
          toast.error(message, { id: "mfa-error" });
          return;
        }

        const redirectUrl = "redirectUrl" in result ? result.redirectUrl : undefined;
        toast.success(t("success.mfaVerified"), { id: "mfa-success" });
        router.replace(redirectUrl ?? getRedirectAfterAuth(input.returnUrl));
      } catch (error) {
        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "mfa-error" });
      }
    });
  }

  function onSubmit(event: React.FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const parsed = schema.safeParse({
      identifier: formData.get("identifier"),
      password: formData.get("password"),
      code,
      challengeId: formData.get("challengeId"),
      returnUrl,
    });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(t("form.fixErrors"));
      return;
    }

    submitMfa(parsed.data);
  }

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
          <FieldLabel>{t("field.code")}</FieldLabel>
          <FieldContent>
            <InputOTP maxLength={6} value={code} onChange={setCode}>
              <InputOTPGroup>
                <InputOTPSlot index={0} />
                <InputOTPSlot index={1} />
                <InputOTPSlot index={2} />
                <InputOTPSlot index={3} />
                <InputOTPSlot index={4} />
                <InputOTPSlot index={5} />
              </InputOTPGroup>
            </InputOTP>
            <FieldError>{fieldErrors.code?.[0]}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? t("form.verifying") : t("common.continue")}
      </Button>
    </form>
  );
}
