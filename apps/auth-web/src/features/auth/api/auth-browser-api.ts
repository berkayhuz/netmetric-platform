import { ApiError } from "@/lib/api/api-error";
import { clientEnv } from "@/lib/env/client-env";
import { isProblemDetails } from "@/lib/api/problem-details";

import type { ConfirmEmailInput, ResendConfirmEmailInput } from "../schemas/confirm-email.schema";
import type { ForgotPasswordInput } from "../schemas/forgot-password.schema";
import type { LoginInput } from "../schemas/login.schema";
import type { MfaInput } from "../schemas/mfa.schema";
import type { RecoveryCodeInput } from "../schemas/recovery-code.schema";
import type { RegisterInput } from "../schemas/register.schema";
import type { ResetPasswordInput } from "../schemas/reset-password.schema";
import type {
  AuthSessionStatus,
  LoginResult,
  LoginSuccessResult,
  RegisterSuccessResult,
} from "../types/auth-session";
import type { WorkspaceSummary, WorkspaceSwitchResult } from "../types/workspace";

type BrowserRequestOptions = {
  method?: "GET" | "POST";
  body?: unknown;
};

type PostLoginDestinationPreference = "Account" | "Tools" | "Crm" | "Public";

type AccountPreferencesResponse = {
  postLoginDestination?: string;
};

const tenantStorageKey = "nm_auth_tenant_id";

function readStoredTenantId(): string | undefined {
  if (typeof window === "undefined") {
    return undefined;
  }

  const value = window.localStorage.getItem(tenantStorageKey);
  return value && value.trim().length > 0 ? value : undefined;
}

function storeTenantId(value: unknown): void {
  if (typeof window === "undefined") {
    return;
  }

  if (typeof value !== "string" || value.trim().length === 0) {
    return;
  }

  window.localStorage.setItem(tenantStorageKey, value);
}

function toLoginPayload(input: {
  email?: string;
  identifier?: string;
  password: string;
  tenantId?: string | undefined;
}) {
  const emailOrUserName = (input.identifier ?? input.email ?? "").trim();

  return {
    tenantId: input.tenantId ?? readStoredTenantId(),
    emailOrUserName,
    password: input.password,
  };
}

function toRegisterPayload(input: RegisterInput) {
  const fullName = input.fullName.trim();
  const [firstName = "", ...rest] = fullName.split(/\s+/);
  const lastName = rest.join(" ").trim();
  const derivedUserName = input.email.trim();
  const tenantName = input.workspaceName?.trim() || `${fullName || derivedUserName}'s workspace`;

  return {
    tenantName,
    userName: derivedUserName,
    email: input.email.trim(),
    password: input.password,
    firstName: firstName || null,
    lastName: lastName || null,
  };
}

function toTenantScopedEmailPayload(input: { email: string; tenantId?: string }) {
  return {
    tenantId: input.tenantId ?? readStoredTenantId(),
    email: input.email.trim(),
  };
}

function normalizeAppUrl(input: string | undefined, fallback: string): string {
  const value = (input?.trim() || fallback).replace(/\/+$/, "");
  return new URL(value).origin;
}

function resolvePostLoginDestinationUrl(destination: string | undefined): string | null {
  const accountOrigin = normalizeAppUrl(clientEnv.NEXT_PUBLIC_ACCOUNT_URL, "http://localhost:7004");
  const toolsOrigin = normalizeAppUrl(clientEnv.NEXT_PUBLIC_TOOLS_URL, "http://localhost:7005");
  const crmOrigin = normalizeAppUrl(clientEnv.NEXT_PUBLIC_CRM_URL, "http://localhost:7006");
  const publicOrigin = normalizeAppUrl(clientEnv.NEXT_PUBLIC_PUBLIC_URL, "http://localhost:7001");

  const normalized = destination?.trim() as PostLoginDestinationPreference | undefined;
  switch (normalized) {
    case "Tools":
      return `${toolsOrigin}/`;
    case "Crm":
      return `${crmOrigin}/`;
    case "Public":
      return `${publicOrigin}/`;
    case "Account":
      return `${accountOrigin}/`;
    default:
      return null;
  }
}

async function readResponsePayload(response: Response): Promise<unknown> {
  const text = await response.text();

  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return text;
  }
}

async function browserRequest<TResponse>(
  path: string,
  options: BrowserRequestOptions = {},
): Promise<TResponse> {
  const init: RequestInit = {
    method: options.method ?? "GET",
    credentials: "include",
    cache: "no-store",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
  };

  if (options.body !== undefined) {
    init.body = JSON.stringify(options.body);
  }

  const response = await fetch(path, init);
  const payload = await readResponsePayload(response);

  if (!response.ok) {
    if (isProblemDetails(payload)) {
      throw new ApiError(
        payload.detail ?? payload.title ?? "Request failed.",
        response.status,
        payload,
      );
    }

    throw new ApiError("Request failed.", response.status);
  }

  return payload as TResponse;
}

async function resolveRedirectFromAccountPreferences(): Promise<string | null> {
  try {
    const preferences = await browserRequest<AccountPreferencesResponse>(
      "/api/auth/post-login-destination",
    );
    return resolvePostLoginDestinationUrl(preferences.postLoginDestination);
  } catch {
    return null;
  }
}

async function withResolvedRedirect(
  result: LoginSuccessResult,
  returnUrl?: string,
): Promise<LoginSuccessResult> {
  if (typeof result.redirectUrl === "string" && result.redirectUrl.trim().length > 0) {
    return result;
  }

  if (returnUrl && returnUrl.trim().length > 0) {
    return result;
  }

  const redirectUrl = await resolveRedirectFromAccountPreferences();
  if (!redirectUrl) {
    return result;
  }

  return { ...result, redirectUrl };
}

export const authBrowserApi = {
  async login(input: LoginInput): Promise<LoginResult> {
    const { returnUrl: _returnUrl } = input;

    void _returnUrl;

    const result = await browserRequest<LoginResult>("/api/auth/login", {
      method: "POST",
      body: toLoginPayload(input),
    });

    if ("tenantId" in result) {
      storeTenantId(result.tenantId);
    }

    if ("mfaRequired" in result) {
      return result;
    }

    return withResolvedRedirect(result, input.returnUrl);
  },

  async verifyMfa(input: MfaInput): Promise<LoginResult> {
    const { code, returnUrl: _returnUrl, ...rest } = input;

    void _returnUrl;

    const result = await browserRequest<LoginResult>("/api/auth/login", {
      method: "POST",
      body: {
        ...toLoginPayload(rest),
        mfaCode: code,
      },
    });

    if ("tenantId" in result) {
      storeTenantId(result.tenantId);
    }

    if ("mfaRequired" in result) {
      return result;
    }

    return withResolvedRedirect(result, input.returnUrl);
  },

  async verifyRecoveryCode(input: RecoveryCodeInput): Promise<LoginResult> {
    const { recoveryCode, returnUrl: _returnUrl, ...rest } = input;

    void _returnUrl;

    const result = await browserRequest<LoginResult>("/api/auth/login", {
      method: "POST",
      body: {
        ...toLoginPayload(rest),
        recoveryCode,
      },
    });

    if ("tenantId" in result) {
      storeTenantId(result.tenantId);
    }

    if ("mfaRequired" in result) {
      return result;
    }

    return withResolvedRedirect(result, input.returnUrl);
  },

  async register(input: RegisterInput): Promise<RegisterSuccessResult> {
    const {
      confirmPassword: _confirmPassword,
      acceptTerms: _acceptTerms,
      returnUrl: _returnUrl,
    } = input;

    void _confirmPassword;
    void _acceptTerms;
    void _returnUrl;

    const result = await browserRequest<RegisterSuccessResult>("/api/auth/register", {
      method: "POST",
      body: toRegisterPayload(input),
    });

    if ("tenantId" in result) {
      storeTenantId(result.tenantId);
    }

    return result;
  },

  forgotPassword(input: ForgotPasswordInput): Promise<void> {
    return browserRequest<void>("/api/auth/forgot-password", {
      method: "POST",
      body: toTenantScopedEmailPayload(input),
    });
  },

  resetPassword(input: ResetPasswordInput): Promise<void> {
    const { confirmPassword: _confirmPassword, password, ...payload } = input;

    void _confirmPassword;

    return browserRequest<void>("/api/auth/reset-password", {
      method: "POST",
      body: {
        ...payload,
        newPassword: password,
      },
    });
  },

  confirmEmail(input: ConfirmEmailInput): Promise<void> {
    return browserRequest<void>("/api/auth/confirm-email", {
      method: "POST",
      body: input,
    });
  },

  resendConfirmEmail(input: ResendConfirmEmailInput): Promise<void> {
    return browserRequest<void>("/api/auth/resend-confirm-email", {
      method: "POST",
      body: input,
    });
  },

  getSession(): Promise<AuthSessionStatus> {
    return browserRequest<AuthSessionStatus>("/api/auth/session");
  },

  getWorkspaces(): Promise<WorkspaceSummary[]> {
    return browserRequest<WorkspaceSummary[]>("/api/auth/workspaces");
  },

  switchWorkspace(tenantId: string): Promise<WorkspaceSwitchResult> {
    return browserRequest<WorkspaceSwitchResult>("/api/auth/workspaces/switch", {
      method: "POST",
      body: {
        tenantId,
      },
    });
  },

  logout(): Promise<void> {
    return browserRequest<void>("/api/auth/logout", {
      method: "POST",
    });
  },
};
