"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition, useEffect } from "react";

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
import { Checkbox, toast } from "@netmetric/ui/client";

import { authBrowserApi } from "@/features/auth/api/auth-browser-api";
import { authRoutes } from "@/features/auth/config/auth-routes";
import { getTranslator } from "@/features/auth/i18n/auth-i18n.client";
import type { Locale } from "@/features/auth/i18n/auth-i18n.shared";
import { createLoginSchema, type LoginInput } from "@/features/auth/schemas/login.schema";
import { getValidationText } from "@/features/auth/schemas/validation-text";
import { getAuthErrorMessage } from "@/features/auth/utils/auth-errors";
import { getRedirectAfterAuth } from "@/features/auth/utils/redirect-after-auth";
import { ApiError } from "@/lib/api/api-error";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

type LoginFormProps = {
  locale: Locale;
  returnUrl?: string;
};

export function LoginForm({ locale, returnUrl }: LoginFormProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const resolvedReturnUrl = useMemo(() => returnUrl ?? "", [returnUrl]);
  const t = useMemo(() => getTranslator(locale), [locale]);
  const schema = useMemo(() => createLoginSchema(getValidationText(locale)), [locale]);

  function isMfaRequiredError(error: unknown): boolean {
    return error instanceof ApiError && error.problem?.errorCode === "mfa_required";
  }

  function navigateToMfa(input: LoginInput, challengeId?: string): void {
    const params = new URLSearchParams();
    params.set("identifier", input.email);

    if (challengeId) {
      params.set("challengeId", challengeId);
    }

    if (input.returnUrl) {
      params.set("returnUrl", input.returnUrl);
    }

    router.replace(`${authRoutes.mfa}?${params.toString()}`);
  }

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);

    if (params.get("passwordReset") === "success") {
      toast.success(t("success.passwordReset"), { id: "login-password-reset-success" });
      params.delete("passwordReset");
      const query = params.toString();
      const nextUrl = query ? `${window.location.pathname}?${query}` : window.location.pathname;
      window.history.replaceState({}, "", nextUrl);
    }
  }, [t]);

  function submitLogin(input: LoginInput): void {
    startTransition(async () => {
      setFormError(null);
      setFieldErrors({});

      try {
        const result = await authBrowserApi.login(input);

        if ("mfaRequired" in result && result.mfaRequired) {
          toast.success(t("success.loginMfaRequired"), { id: "login-mfa-required" });
          navigateToMfa(input, result.challengeId);
          return;
        }

        const redirectUrl = "redirectUrl" in result ? result.redirectUrl : undefined;
        toast.success(t("success.login"), { id: "login-success" });
        router.replace(redirectUrl ?? getRedirectAfterAuth(input.returnUrl));
      } catch (error) {
        if (isMfaRequiredError(error)) {
          toast.success(t("success.loginMfaRequired"), { id: "login-mfa-required" });
          navigateToMfa(input);
          return;
        }

        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "login-error" });
      }
    });
  }

  function onSubmit(event: React.FormEvent<HTMLFormElement>): void {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    const tenantIdValue = formData.get("tenantId");
    const parsed = schema.safeParse({
      email: formData.get("email"),
      password: formData.get("password"),
      rememberMe: formData.get("rememberMe") === "on",
      tenantId: typeof tenantIdValue === "string" ? tenantIdValue : undefined,
      returnUrl: resolvedReturnUrl,
    });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(t("form.fixErrors"));
      return;
    }

    submitLogin(parsed.data);
  }

  return (
    <form onSubmit={onSubmit} className="space-y-5" noValidate>
      {formError ? (
        <Alert variant="destructive">
          <AlertDescription>{formError}</AlertDescription>
        </Alert>
      ) : null}

      <input type="hidden" name="returnUrl" value={resolvedReturnUrl} />

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

        <Field>
          <div className="flex items-center justify-between gap-4">
            <FieldLabel htmlFor="password">{t("field.password")}</FieldLabel>
            <Link
              href={authRoutes.forgotPassword}
              className="text-sm font-medium text-muted-foreground transition hover:text-foreground"
            >
              {t("link.forgotPassword")}
            </Link>
          </div>
          <FieldContent>
            <Input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              aria-invalid={Boolean(fieldErrors.password?.[0])}
            />
            <FieldError>{fieldErrors.password?.[0]}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <label className="flex items-center gap-2 text-sm text-muted-foreground">
        <Checkbox name="rememberMe" />
        {t("field.rememberMe")}
      </label>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? t("action.loggingIn") : t("action.login")}
      </Button>
    </form>
  );
}
