"use server";

import { redirect } from "next/navigation";

import { fail, ok, type ApiResult } from "@/lib/api/api-result";
import { assertSameOriginRequest } from "@/lib/security/csrf";
import { toFieldErrors } from "@/lib/validation/zod-error-map";

import { authApi } from "../api/auth-api";
import { authRoutes } from "../config/auth-routes";
import { getRequestLocale, getTranslator } from "../i18n/auth-i18n.server";
import { createForgotPasswordSchema } from "../schemas/forgot-password.schema";
import { createLoginSchema } from "../schemas/login.schema";
import { createRegisterSchema } from "../schemas/register.schema";
import { createResetPasswordSchema } from "../schemas/reset-password.schema";
import { getValidationText } from "../schemas/validation-text";
import { createSwitchWorkspaceSchema } from "../schemas/workspace.schema";
import type { LoginResult, RegisterSuccessResult } from "../types/auth-session";
import type { WorkspaceSwitchResult } from "../types/workspace";
import { getAuthErrorMessage } from "../utils/auth-errors";
import { getRedirectAfterAuth } from "../utils/redirect-after-auth";

function getStringValue(formData: FormData, key: string): string | undefined {
  const value = formData.get(key);
  return typeof value === "string" ? value : undefined;
}

async function getMessages() {
  const locale = await getRequestLocale();
  const t = getTranslator(locale);
  return { t, validation: getValidationText(locale) };
}

export async function loginAction(formData: FormData): Promise<ApiResult<LoginResult>> {
  await assertSameOriginRequest();
  const { t, validation } = await getMessages();

  const parsed = createLoginSchema(validation).safeParse({
    email: getStringValue(formData, "email"),
    password: getStringValue(formData, "password"),
    tenantId: getStringValue(formData, "tenantId"),
    rememberMe: getStringValue(formData, "rememberMe") === "on",
    returnUrl: getStringValue(formData, "returnUrl"),
  });
  if (!parsed.success) {
    return fail({
      status: 400,
      message: t("form.fixErrors"),
      fieldErrors: toFieldErrors(parsed.error),
    });
  }

  const { returnUrl, ...input } = parsed.data;

  try {
    const loginInput = {
      email: input.email,
      password: input.password,
      rememberMe: input.rememberMe,
      ...(input.tenantId ? { tenantId: input.tenantId } : {}),
    };

    const result = await authApi.login(loginInput);

    if ("mfaRequired" in result && result.mfaRequired) {
      return ok(result);
    }

    const redirectUrl = "redirectUrl" in result ? result.redirectUrl : undefined;
    redirect(redirectUrl ?? getRedirectAfterAuth(returnUrl));
  } catch (error) {
    return fail({ status: 400, message: getAuthErrorMessage(error) });
  }
}

export async function registerAction(
  formData: FormData,
): Promise<ApiResult<RegisterSuccessResult>> {
  await assertSameOriginRequest();
  const { t, validation } = await getMessages();

  const parsed = createRegisterSchema(validation).safeParse({
    fullName: getStringValue(formData, "fullName"),
    email: getStringValue(formData, "email"),
    password: getStringValue(formData, "password"),
    confirmPassword: getStringValue(formData, "confirmPassword"),
    workspaceName: getStringValue(formData, "workspaceName"),
    acceptTerms: getStringValue(formData, "acceptTerms") === "on",
    returnUrl: getStringValue(formData, "returnUrl"),
  });
  if (!parsed.success) {
    return fail({
      status: 400,
      message: t("form.fixErrors"),
      fieldErrors: toFieldErrors(parsed.error),
    });
  }

  const {
    confirmPassword: _confirmPassword,
    acceptTerms: _acceptTerms,
    returnUrl,
    ...input
  } = parsed.data;
  void _confirmPassword;
  void _acceptTerms;

  try {
    const registerInput = {
      fullName: input.fullName,
      email: input.email,
      password: input.password,
      ...(input.workspaceName ? { workspaceName: input.workspaceName } : {}),
    };

    const result = await authApi.register(registerInput);
    const redirectUrl = result.redirectUrl ?? getRedirectAfterAuth(returnUrl);

    if (result.emailConfirmationRequired) {
      redirect(authRoutes.confirmEmail);
    }

    redirect(redirectUrl);
  } catch (error) {
    return fail({ status: 400, message: getAuthErrorMessage(error) });
  }
}

export async function forgotPasswordAction(formData: FormData): Promise<ApiResult<void>> {
  await assertSameOriginRequest();
  const { t, validation } = await getMessages();
  const parsed = createForgotPasswordSchema(validation).safeParse({
    email: getStringValue(formData, "email"),
  });

  if (!parsed.success) {
    return fail({
      status: 400,
      message: t("form.fixErrors"),
      fieldErrors: toFieldErrors(parsed.error),
    });
  }

  try {
    await authApi.forgotPassword(parsed.data);
    return ok(undefined);
  } catch (error) {
    return fail({ status: 400, message: getAuthErrorMessage(error) });
  }
}

export async function resetPasswordAction(formData: FormData): Promise<ApiResult<void>> {
  await assertSameOriginRequest();
  const { t, validation } = await getMessages();

  const parsed = createResetPasswordSchema(validation).safeParse({
    tenantId: getStringValue(formData, "tenantId"),
    userId: getStringValue(formData, "userId"),
    token: getStringValue(formData, "token"),
    password: getStringValue(formData, "password"),
    confirmPassword: getStringValue(formData, "confirmPassword"),
  });

  if (!parsed.success) {
    return fail({
      status: 400,
      message: t("form.fixErrors"),
      fieldErrors: toFieldErrors(parsed.error),
    });
  }

  const { confirmPassword: _confirmPassword, password, ...input } = parsed.data;
  void _confirmPassword;

  try {
    await authApi.resetPassword({
      ...input,
      newPassword: password,
    });
    return ok(undefined);
  } catch (error) {
    return fail({ status: 400, message: getAuthErrorMessage(error) });
  }
}

export async function switchWorkspaceAction(
  formData: FormData,
): Promise<ApiResult<WorkspaceSwitchResult>> {
  await assertSameOriginRequest();
  const { t, validation } = await getMessages();

  const parsed = createSwitchWorkspaceSchema(validation).safeParse({
    tenantId: getStringValue(formData, "tenantId"),
    returnUrl: getStringValue(formData, "returnUrl"),
  });

  if (!parsed.success) {
    return fail({
      status: 400,
      message: t("validation.workspace.selectOne"),
      fieldErrors: toFieldErrors(parsed.error),
    });
  }

  const { returnUrl, ...input } = parsed.data;

  try {
    const result = await authApi.switchWorkspace(input);
    const redirectUrl = "redirectUrl" in result ? result.redirectUrl : undefined;
    redirect(redirectUrl ?? getRedirectAfterAuth(returnUrl));
  } catch (error) {
    return fail({ status: 400, message: getAuthErrorMessage(error) });
  }
}

export async function logoutAction(): Promise<void> {
  await assertSameOriginRequest();

  try {
    await authApi.logout();
  } finally {
    redirect(authRoutes.login);
  }
}
