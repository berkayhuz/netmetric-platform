import "server-only";

import { apiRequest } from "@/lib/api/api-client";

import { authEndpoints } from "./auth-endpoints";
import type { AuthSessionStatus, LoginResult, RegisterSuccessResult } from "../types/auth-session";
import type { WorkspaceSummary, WorkspaceSwitchResult } from "../types/workspace";

export type LoginRequest = {
  email: string;
  password: string;
  tenantId?: string;
  rememberMe?: boolean;
  mfaCode?: string;
  recoveryCode?: string;
};

export type RegisterRequest = {
  fullName: string;
  email: string;
  password: string;
  workspaceName?: string;
};

function toRegisterPayload(input: RegisterRequest) {
  const fullName = input.fullName.trim();
  const [firstName = "", ...rest] = fullName.split(/\s+/);
  const lastName = rest.join(" ").trim();
  const userName = input.email.trim();
  const tenantName = input.workspaceName?.trim() || `${fullName || userName}'s workspace`;

  return {
    tenantName,
    userName,
    email: input.email.trim(),
    password: input.password,
    firstName: firstName || null,
    lastName: lastName || null,
  };
}

export type ForgotPasswordRequest = {
  email: string;
};

export type ResetPasswordRequest = {
  tenantId: string;
  userId: string;
  token: string;
  newPassword: string;
};

export type ConfirmEmailRequest = {
  userId: string;
  token: string;
};

export type SwitchWorkspaceRequest = {
  tenantId: string;
};

export type SessionStatusResponse = {
  tenantId?: unknown;
  userId?: unknown;
  email?: unknown;
};

export function mapSessionStatusToAuthSessionStatus(
  payload: SessionStatusResponse,
): AuthSessionStatus {
  if (
    typeof payload.tenantId !== "string" ||
    typeof payload.userId !== "string" ||
    typeof payload.email !== "string"
  ) {
    return { authenticated: false };
  }

  return {
    authenticated: true,
    activeTenantId: payload.tenantId,
    user: {
      id: payload.userId,
      email: payload.email,
      displayName: payload.email,
    },
  };
}

export const authApi = {
  login(input: LoginRequest): Promise<LoginResult> {
    return apiRequest<LoginResult>(authEndpoints.login, {
      method: "POST",
      body: input,
    });
  },

  register(input: RegisterRequest): Promise<RegisterSuccessResult> {
    return apiRequest<RegisterSuccessResult>(authEndpoints.register, {
      method: "POST",
      body: toRegisterPayload(input),
    });
  },

  forgotPassword(input: ForgotPasswordRequest): Promise<void> {
    return apiRequest<void>(authEndpoints.forgotPassword, {
      method: "POST",
      body: input,
    });
  },

  resetPassword(input: ResetPasswordRequest): Promise<void> {
    return apiRequest<void>(authEndpoints.resetPassword, {
      method: "POST",
      body: input,
    });
  },

  confirmEmail(input: ConfirmEmailRequest): Promise<void> {
    return apiRequest<void>(authEndpoints.confirmEmail, {
      method: "POST",
      body: input,
    });
  },

  resendConfirmEmail(input: ForgotPasswordRequest): Promise<void> {
    return apiRequest<void>(authEndpoints.resendConfirmEmail, {
      method: "POST",
      body: input,
    });
  },

  getSessionStatus(options: { timeoutMs?: number } = {}): Promise<AuthSessionStatus> {
    return apiRequest<SessionStatusResponse>(authEndpoints.sessionStatus, {
      method: "GET",
      ...(options.timeoutMs !== undefined ? { timeoutMs: options.timeoutMs } : {}),
    }).then(mapSessionStatusToAuthSessionStatus);
  },

  getWorkspaces(): Promise<WorkspaceSummary[]> {
    return apiRequest<WorkspaceSummary[]>(authEndpoints.workspaces, {
      method: "GET",
    });
  },

  switchWorkspace(input: SwitchWorkspaceRequest): Promise<WorkspaceSwitchResult> {
    return apiRequest<WorkspaceSwitchResult>(authEndpoints.switchWorkspace, {
      method: "POST",
      body: input,
    });
  },

  logout(): Promise<void> {
    return apiRequest<void>(authEndpoints.logout, {
      method: "POST",
    });
  },
};
