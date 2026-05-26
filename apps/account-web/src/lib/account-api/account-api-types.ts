export type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export type AccountApiAuthContext = {
  bearerToken?: string;
  cookieHeader?: string;
  csrfToken?: string;
};

export type AccountApiRequestOptions = {
  authContext?: AccountApiAuthContext;
  correlationId?: string;
  timeoutMs?: number;
  signal?: AbortSignal;
};

export type OrganizationMembershipSummaryResponse = {
  organizationId: string;
  tenantId: string;
  organizationName: string;
  organizationSlug?: string | null;
  status: string;
  isDefault: boolean;
  joinedAt: string;
  lastPermissionRefreshAt?: string | null;
  roles: string[];
};

export type AccountOverviewResponse = {
  displayName: string;
  avatarUrl?: string | null;
  isMfaEnabled: boolean;
  activeSessionCount: number;
  organizations: OrganizationMembershipSummaryResponse[];
  lastSecurityEventAt?: string | null;
};

export type MyProfileResponse = {
  id: string;
  tenantId: string;
  userId: string;
  firstName: string;
  lastName: string;
  displayName: string;
  phoneNumber?: string | null;
  phoneCountryIso2?: string | null;
  phoneCountryCallingCode?: string | null;
  phoneNationalNumber?: string | null;
  avatarUrl?: string | null;
  jobTitle?: string | null;
  department?: string | null;
  timeZone: string;
  culture: string;
  version: string;
};

export type UpdateMyProfileRequest = {
  firstName: string;
  lastName: string;
  phoneCountryIso2?: string | null;
  phoneNationalNumber?: string | null;
  avatarUrl?: string | null;
  jobTitle?: string | null;
  department?: string | null;
  timeZone: string;
  culture: string;
  version?: string | null;
};

export type UserPreferenceResponse = {
  id: string;
  theme: string;
  language: string;
  timeZone: string;
  dateFormat: string;
  postLoginDestination: string;
  defaultOrganizationId?: string | null;
  faviconUrl?: string | null;
  crmDashboardPreferencesJson?: string | null;
  version: string;
};

export type AccountOptionItem = {
  value: string;
  label: string;
};

export type CountryCallingCodeOption = {
  iso2: string;
  name: string;
  dialCode: string;
};

export type AccountOptionsResponse = {
  languages: AccountOptionItem[];
  timeZones: AccountOptionItem[];
  themes: AccountOptionItem[];
  dateFormats: AccountOptionItem[];
  phoneCountries: CountryCallingCodeOption[];
};

export type UpdateUserPreferenceRequest = {
  theme: string;
  language: string;
  timeZone: string;
  dateFormat: string;
  postLoginDestination: string;
  defaultOrganizationId?: string | null;
  crmDashboardPreferencesJson?: string | null;
  version?: string | null;
};

export type UserSessionResponse = {
  id: string;
  deviceName?: string | null;
  ipAddress?: string | null;
  userAgent: string;
  approximateLocation?: string | null;
  createdAt: string;
  lastSeenAt: string;
  expiresAt: string;
  isCurrent: boolean;
  isActive: boolean;
};

export type UserSessionsResponse = {
  items: UserSessionResponse[];
};

export type TrustedDeviceResponse = {
  id: string;
  name: string;
  ipAddress?: string | null;
  userAgent: string;
  trustedAt: string;
  expiresAt: string;
  isCurrent: boolean;
  isActive: boolean;
};

export type TrustedDevicesResponse = {
  items: TrustedDeviceResponse[];
};

export type ChangePasswordRequest = {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
};

export type PasswordPolicyFailureResponse = {
  code: string;
  message: string;
};

export type MfaStatusResponse = {
  isEnabled: boolean;
  hasAuthenticator: boolean;
  recoveryCodesRemaining: number;
};

export type MfaSetupResponse = {
  sharedKey: string;
  authenticatorUri: string;
};

export type ConfirmMfaRequest = {
  verificationCode: string;
};

export type ConfirmMfaResponse = {
  isEnabled: boolean;
  recoveryCodes: string[];
};

export type DisableMfaRequest = {
  verificationCode: string;
};

export type RecoveryCodesResponse = {
  recoveryCodes: string[];
};

export type EmailChangeRequest = {
  newEmail: string;
  currentPassword: string;
};

export type EmailChangeConfirmRequest = {
  token: string;
};

export type EmailChangeRequestResponse = {
  confirmationRequired: boolean;
};

export type EmailChangeConfirmResponse = {
  newEmail: string;
};

export type AccountNotificationResponse = {
  id: string;
  title: string;
  description: string;
  category: string;
  severity: string;
  occurredAt: string;
  isRead: boolean;
};

export type AccountNotificationsResponse = {
  items: AccountNotificationResponse[];
  totalCount: number;
  unreadCount: number;
  readCount: number;
};

export type NotificationPreferenceItemResponse = {
  id: string;
  channel: string;
  category: string;
  isEnabled: boolean;
  version: string;
};

export type NotificationPreferencesResponse = {
  items: NotificationPreferenceItemResponse[];
};

export type UpdateNotificationPreferenceItemRequest = {
  channel: string;
  category: string;
  isEnabled: boolean;
};

export type UpdateNotificationPreferencesRequest = {
  items: UpdateNotificationPreferenceItemRequest[];
};

export type AccountAuditEntryResponse = {
  id: string;
  tenantId: string;
  userId: string;
  eventType: string;
  severity: string;
  occurredAt: string;
  correlationId?: string | null;
  metadataJson?: string | null;
};

export type AccountAuditEntriesResponse = {
  items: AccountAuditEntryResponse[];
  count: number;
};

export type ConsentHistoryItemResponse = {
  id: string;
  consentType: string;
  version: string;
  status: string;
  decidedAt: string;
};

export type ConsentsResponse = {
  items: ConsentHistoryItemResponse[];
};

export type AcceptConsentRequest = {
  version: string;
};

export type CreateAccountInvitationRequest = {
  email: string;
  firstName?: string | null;
  lastName?: string | null;
};

export type AccountInvitationResponse = {
  tenantId: string;
  invitationId: string;
  email: string;
  expiresAtUtc: string;
  status: string;
  lastSentAtUtc?: string | null;
};

export type AccountInvitationSummaryResponse = {
  tenantId: string;
  invitationId: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  expiresAtUtc: string;
  status: string;
  resendCount: number;
  createdAtUtc: string;
  lastSentAtUtc?: string | null;
  acceptedAtUtc?: string | null;
  revokedAtUtc?: string | null;
  lastDeliveryStatus?: string | null;
};

export type AccountMemberResponse = {
  tenantId: string;
  userId: string;
  userName: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  isActive: boolean;
  roles: string[];
  permissions: string[];
  createdAt: string;
  lastLoginAt?: string | null;
};

export type AccountRoleCatalogResponse = {
  name: string;
  rank: number;
  isProtected: boolean;
  permissions: string[];
};

export type UpdateAccountMemberRolesRequest = {
  roles: string[];
};

export type AvatarUploadResponse = {
  assetId: string;
  publicUrl: string;
  contentType: string;
  sizeBytes: number;
  width?: number | null;
  height?: number | null;
  status: string;
  purpose: string;
  createdAtUtc: string;
};
