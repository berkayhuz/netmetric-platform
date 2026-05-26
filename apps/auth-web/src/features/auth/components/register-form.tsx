"use client";

import Link from "next/link";
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
import { Checkbox, toast } from "@netmetric/ui/client";

import { authBrowserApi } from "@/features/auth/api/auth-browser-api";
import { authRoutes, externalRoutes } from "@/features/auth/config/auth-routes";
import { getTranslator } from "@/features/auth/i18n/auth-i18n.client";
import type { Locale } from "@/features/auth/i18n/auth-i18n.shared";
import { createRegisterSchema, type RegisterInput } from "@/features/auth/schemas/register.schema";
import { getValidationText } from "@/features/auth/schemas/validation-text";
import { getAuthErrorMessage } from "@/features/auth/utils/auth-errors";
import { getRedirectAfterAuth } from "@/features/auth/utils/redirect-after-auth";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

type RegisterFormProps = { locale: Locale; returnUrl?: string };

export function RegisterForm({ locale, returnUrl }: RegisterFormProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [acceptTerms, setAcceptTerms] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const t = useMemo(() => getTranslator(locale), [locale]);
  const schema = useMemo(() => createRegisterSchema(getValidationText(locale)), [locale]);

  function submitRegister(input: RegisterInput): void {
    startTransition(async () => {
      setFormError(null);
      setFieldErrors({});

      try {
        const result = await authBrowserApi.register(input);
        toast.success(t("success.register"), { id: "register-success" });

        if (result.emailConfirmationRequired) {
          const params = new URLSearchParams();
          params.set("email", input.email);
          const tenantId =
            "tenantId" in result && typeof result.tenantId === "string" ? result.tenantId : "";
          if (tenantId) {
            params.set("tenantId", tenantId);
          }
          router.replace(`${authRoutes.confirmEmail}?${params.toString()}`);
          return;
        }

        router.replace(result.redirectUrl ?? getRedirectAfterAuth(input.returnUrl));
      } catch (error) {
        const message = getAuthErrorMessage(error, locale);
        setFormError(message);
        toast.error(message, { id: "register-error" });
      }
    });
  }

  function onSubmit(event: React.FormEvent<HTMLFormElement>): void {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    const parsed = schema.safeParse({
      fullName: formData.get("fullName"),
      email: formData.get("email"),
      password: formData.get("password"),
      confirmPassword: formData.get("confirmPassword"),
      workspaceName: formData.get("workspaceName"),
      acceptTerms,
      returnUrl,
    });

    if (!parsed.success) {
      setFieldErrors(toFieldErrors(parsed.error));
      setFormError(t("form.fixErrors"));
      return;
    }

    submitRegister(parsed.data);
  }

  return (
    <form onSubmit={onSubmit} className="space-y-5" noValidate>
      {formError ? (
        <Alert variant="destructive">
          <AlertDescription>{formError}</AlertDescription>
        </Alert>
      ) : null}

      <FieldSet>
        <Field>
          <FieldLabel htmlFor="fullName">{t("field.fullName")}</FieldLabel>
          <FieldContent>
            <Input id="fullName" name="fullName" autoComplete="name" />
            <FieldError>{fieldErrors.fullName?.[0]}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="email">{t("field.email")}</FieldLabel>
          <FieldContent>
            <Input id="email" name="email" type="email" autoComplete="email" />
            <FieldError>{fieldErrors.email?.[0]}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="password">{t("field.password")}</FieldLabel>
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
        <Field>
          <FieldLabel htmlFor="workspaceName">{t("field.workspaceName")}</FieldLabel>
          <FieldContent>
            <Input id="workspaceName" name="workspaceName" />
            <FieldError>{fieldErrors.workspaceName?.[0]}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <label className="flex items-center gap-2 text-sm text-muted-foreground">
        <Checkbox
          checked={acceptTerms}
          onCheckedChange={(checked) => setAcceptTerms(checked === true)}
        />
        <span>
          {t("field.acceptTerms")} (
          <Link href={externalRoutes.terms} className="underline">
            {t("nav.terms")}
          </Link>
          )
        </span>
      </label>
      <FieldError>{fieldErrors.acceptTerms?.[0]}</FieldError>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? t("action.registering") : t("action.register")}
      </Button>
    </form>
  );
}
