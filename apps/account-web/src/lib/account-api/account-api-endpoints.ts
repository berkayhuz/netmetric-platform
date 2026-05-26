import "server-only";

import type { HttpMethod } from "./account-api-types";

export type AccountApiEndpoint = {
  method: HttpMethod;
  path: string;
};

export const accountApiEndpoints = {
  overview: { method: "GET", path: "/api/v1/account/overview" },
  optionsGet: { method: "GET", path: "/api/v1/account/options" },
  profileGet: { method: "GET", path: "/api/v1/account/profile" },
  profileUpdate: { method: "PUT", path: "/api/v1/account/profile" },
  profileAvatarUpload: { method: "POST", path: "/api/v1/account/profile/avatar" },
  profileAvatarDelete: { method: "DELETE", path: "/api/v1/account/profile/avatar" },
  preferencesGet: { method: "GET", path: "/api/v1/account/preferences" },
  preferencesUpdate: { method: "PUT", path: "/api/v1/account/preferences" },
  preferencesFaviconUpload: { method: "POST", path: "/api/v1/account/preferences/favicon" },
  preferencesFaviconDelete: { method: "DELETE", path: "/api/v1/account/preferences/favicon" },
  sessionsGet: { method: "GET", path: "/api/v1/account/sessions" },
  sessionsRevoke: (sessionId: string): AccountApiEndpoint => ({
    method: "DELETE",
    path: `/api/v1/account/sessions/${encodeURIComponent(sessionId)}`,
  }),
  sessionsRevokeOthers: { method: "POST", path: "/api/v1/account/sessions/revoke-others" },
  trustedDevicesGet: { method: "GET", path: "/api/v1/account/devices/trusted" },
  trustedDevicesRevoke: (deviceId: string): AccountApiEndpoint => ({
    method: "DELETE",
    path: `/api/v1/account/devices/trusted/${encodeURIComponent(deviceId)}`,
  }),
  trustedDevicesRevokeOthers: {
    method: "POST",
    path: "/api/v1/account/devices/trusted/revoke-others",
  },
  securityPasswordChange: { method: "POST", path: "/api/v1/account/security/password/change" },
  securityEmailChangeRequest: {
    method: "POST",
    path: "/api/v1/account/security/email/change-request",
  },
  securityEmailChangeConfirm: {
    method: "POST",
    path: "/api/v1/account/security/email/change-confirm",
  },
  securityMfaGet: { method: "GET", path: "/api/v1/account/security/mfa" },
  securityMfaSetup: { method: "POST", path: "/api/v1/account/security/mfa/setup" },
  securityMfaConfirm: { method: "POST", path: "/api/v1/account/security/mfa/confirm" },
  securityMfaDisable: { method: "DELETE", path: "/api/v1/account/security/mfa" },
  securityMfaRecoveryRegenerate: {
    method: "POST",
    path: "/api/v1/account/security/mfa/recovery-codes/regenerate",
  },
  notificationsGet: { method: "GET", path: "/api/v1/account/notifications" },
  notificationsMarkRead: (notificationId: string): AccountApiEndpoint => ({
    method: "POST",
    path: `/api/v1/account/notifications/${encodeURIComponent(notificationId)}/read`,
  }),
  notificationsMarkAllRead: { method: "POST", path: "/api/v1/account/notifications/mark-all-read" },
  notificationsDelete: (notificationId: string): AccountApiEndpoint => ({
    method: "DELETE",
    path: `/api/v1/account/notifications/${encodeURIComponent(notificationId)}`,
  }),
  notificationPreferencesGet: {
    method: "GET",
    path: "/api/v1/account/notifications/preferences",
  },
  notificationPreferencesUpdate: {
    method: "PUT",
    path: "/api/v1/account/notifications/preferences",
  },
  auditList: { method: "GET", path: "/api/v1/account/audit" },
  consentsGet: { method: "GET", path: "/api/v1/account/consents" },
  consentsAccept: (consentType: string): AccountApiEndpoint => ({
    method: "POST",
    path: `/api/v1/account/consents/${encodeURIComponent(consentType)}/accept`,
  }),
  invitationsCreate: { method: "POST", path: "/api/v1/account/invitations" },
  invitationsList: { method: "GET", path: "/api/v1/account/invitations" },
  invitationsResend: (invitationId: string): AccountApiEndpoint => ({
    method: "POST",
    path: `/api/v1/account/invitations/${encodeURIComponent(invitationId)}/resend`,
  }),
  invitationsRevoke: (invitationId: string): AccountApiEndpoint => ({
    method: "POST",
    path: `/api/v1/account/invitations/${encodeURIComponent(invitationId)}/revoke`,
  }),
  membersList: { method: "GET", path: "/api/v1/account/members" },
  rolesCatalogList: { method: "GET", path: "/api/v1/account/roles/catalog" },
  memberRolesUpdate: (userId: string): AccountApiEndpoint => ({
    method: "PUT",
    path: `/api/v1/account/members/${encodeURIComponent(userId)}/roles`,
  }),
} as const;
