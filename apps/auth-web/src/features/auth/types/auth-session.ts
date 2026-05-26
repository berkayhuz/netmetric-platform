import type { WorkspaceSummary } from "./workspace";

export type AuthUser = {
  id: string;
  email: string;
  displayName?: string;
  emailConfirmed?: boolean;
  mfaEnabled?: boolean;
};

export type AuthSessionStatus = {
  authenticated: boolean;
  user?: AuthUser;
  activeTenantId?: string;
  activeWorkspace?: WorkspaceSummary;
  workspaces?: WorkspaceSummary[];
  expiresAt?: string;
};

export type LoginSuccessResult = {
  accessTokenExpiresAt?: string;
  user?: AuthUser;
  activeTenantId?: string;
  workspaces?: WorkspaceSummary[];
  requiresWorkspaceSelection?: boolean;
  redirectUrl?: string;
};

export type RegisterSuccessResult = {
  userId?: string;
  email?: string;
  emailConfirmationRequired?: boolean;
  redirectUrl?: string;
};

export type MfaRequiredResult = {
  mfaRequired: true;
  challengeId?: string;
  availableMethods?: string[];
};

export type LoginResult = LoginSuccessResult | MfaRequiredResult;
