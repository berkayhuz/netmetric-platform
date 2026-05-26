export const authEndpoints = {
  register: "/api/auth/register",
  login: "/api/auth/login",
  refresh: "/api/auth/refresh",
  logout: "/api/auth/logout",
  forgotPassword: "/api/auth/forgot-password",
  resetPassword: "/api/auth/reset-password",
  confirmEmail: "/api/auth/confirm-email",
  resendConfirmEmail: "/api/auth/resend-confirm-email",
  confirmEmailChange: "/api/auth/confirm-email-change",
  workspaces: "/api/auth/workspaces",
  switchWorkspace: "/api/auth/workspaces/switch",
  acceptInvitation: "/api/auth/invitations/accept",
  sessionStatus: "/api/auth/session-status",
} as const;

export type AuthEndpoint = (typeof authEndpoints)[keyof typeof authEndpoints];
