import "server-only";

import { createServerPerformanceLogger } from "@netmetric/observability/server";

import { getAccountApiConfig, joinAccountApiPath } from "./account-api-config";
import { accountApiEndpoints } from "./account-api-endpoints";
import { AccountApiError, statusToAccountApiErrorKind } from "./account-api-errors";
import { applyCorrelationId, getCorrelationIdFromHeaders } from "./correlation";
import { normalizeProblemDetails } from "./problem-details";
import type {
  AcceptConsentRequest,
  AccountApiAuthContext,
  AccountOptionsResponse,
  AccountApiRequestOptions,
  AccountAuditEntriesResponse,
  AccountInvitationResponse,
  AccountInvitationSummaryResponse,
  AccountMemberResponse,
  AccountNotificationsResponse,
  AccountOverviewResponse,
  AccountRoleCatalogResponse,
  AvatarUploadResponse,
  ChangePasswordRequest,
  ConfirmMfaRequest,
  ConfirmMfaResponse,
  ConsentsResponse,
  CreateAccountInvitationRequest,
  DisableMfaRequest,
  EmailChangeConfirmRequest,
  EmailChangeConfirmResponse,
  EmailChangeRequest,
  EmailChangeRequestResponse,
  HttpMethod,
  MfaSetupResponse,
  MfaStatusResponse,
  MyProfileResponse,
  NotificationPreferencesResponse,
  RecoveryCodesResponse,
  TrustedDevicesResponse,
  UpdateAccountMemberRolesRequest,
  UpdateMyProfileRequest,
  UpdateNotificationPreferencesRequest,
  UpdateUserPreferenceRequest,
  UserPreferenceResponse,
  UserSessionsResponse,
} from "./account-api-types";

const accountApiPerformance = createServerPerformanceLogger({
  app: "account-web",
  component: "account-api-client",
  enabled: process.env.NETMETRIC_PERF_LOG === "1" || process.env.NODE_ENV !== "production",
});

type RequestOptions = AccountApiRequestOptions & {
  method: HttpMethod;
  path: string;
  body?: unknown;
  query?: Record<string, string | number | boolean | undefined>;
  contentType?: string;
};

function toTelemetryPath(pathWithQuery: string): string {
  return pathWithQuery.split("?")[0] ?? pathWithQuery;
}

function withQuery(
  path: string,
  query: Record<string, string | number | boolean | undefined>,
): string {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(query)) {
    if (value === undefined) {
      continue;
    }

    params.set(key, String(value));
  }

  const suffix = params.toString();
  return suffix ? `${path}?${suffix}` : path;
}

function readBodyAsJson(body: unknown, contentType?: string): BodyInit | undefined {
  if (body === undefined) {
    return undefined;
  }

  if (body instanceof FormData) {
    return body;
  }

  if (contentType && contentType !== "application/json") {
    return body as BodyInit;
  }

  return JSON.stringify(body);
}

async function parseResponsePayload(response: Response): Promise<unknown> {
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

function buildHeaders(
  authContext: AccountApiAuthContext | undefined,
  correlationId: string | undefined,
): Headers {
  const headers = new Headers();
  headers.set("accept", "application/json");

  if (authContext?.bearerToken) {
    headers.set("authorization", `Bearer ${authContext.bearerToken}`);
  }

  if (authContext?.cookieHeader) {
    headers.set("cookie", authContext.cookieHeader);
  }

  if (authContext?.csrfToken) {
    headers.set("x-csrf-token", authContext.csrfToken);
  }

  applyCorrelationId(headers, correlationId);
  return headers;
}

function createTimeoutSignal(timeoutMs: number, parentSignal?: AbortSignal): AbortSignal {
  const timeoutSignal = AbortSignal.timeout(timeoutMs);

  if (!parentSignal) {
    return timeoutSignal;
  }

  if (parentSignal.aborted) {
    return parentSignal;
  }

  const controller = new AbortController();
  const abort = () => controller.abort();

  parentSignal.addEventListener("abort", abort, { once: true });
  timeoutSignal.addEventListener("abort", abort, { once: true });

  return controller.signal;
}

async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
  const pathWithQuery = options.query ? withQuery(options.path, options.query) : options.path;
  const requestUrl = joinAccountApiPath(pathWithQuery);
  const correlationId = options.correlationId;
  const headers = buildHeaders(options.authContext, correlationId);
  const body = readBodyAsJson(options.body, options.contentType);

  if (body && !options.contentType && !(body instanceof FormData)) {
    headers.set("content-type", "application/json");
  } else if (options.contentType) {
    headers.set("content-type", options.contentType);
  }

  const signal = createTimeoutSignal(
    options.timeoutMs ?? getAccountApiConfig().defaultTimeoutMs,
    options.signal,
  );
  const requestInit: RequestInit = {
    method: options.method,
    headers,
    cache: "no-store",
    signal,
    redirect: "manual",
  };

  if (body !== undefined) {
    requestInit.body = body;
  }

  let response: Response;
  const telemetryPath = toTelemetryPath(pathWithQuery);
  const startedAt = performance.now();
  try {
    response = await fetch(requestUrl, requestInit);
  } catch {
    accountApiPerformance.record("fetch.error", performance.now() - startedAt, {
      method: options.method,
      path: telemetryPath,
      status: "network_error",
    });
    const errorInput: ConstructorParameters<typeof AccountApiError>[0] = {
      message: "Account API is unavailable.",
      status: 503,
      kind: "upstream_unavailable",
    };
    if (correlationId) {
      errorInput.correlationId = correlationId;
    }
    throw new AccountApiError(errorInput);
  }
  accountApiPerformance.record("fetch", performance.now() - startedAt, {
    method: options.method,
    path: telemetryPath,
    status: String(response.status),
    serverTiming: response.headers.get("server-timing") ?? "none",
  });

  const payload = await parseResponsePayload(response);
  const responseCorrelationId = getCorrelationIdFromHeaders(response.headers) ?? correlationId;

  if (!response.ok) {
    const problem = normalizeProblemDetails(payload);

    const errorInput: ConstructorParameters<typeof AccountApiError>[0] = {
      message: problem?.detail ?? problem?.title ?? "Account API request failed.",
      status: response.status,
      kind: statusToAccountApiErrorKind(response.status),
    };

    if (problem) {
      errorInput.problem = problem;
    }

    if (responseCorrelationId) {
      errorInput.correlationId = responseCorrelationId;
    }

    throw new AccountApiError(errorInput);
  }

  if (response.status === 204 || payload === null) {
    return undefined as TResponse;
  }

  return payload as TResponse;
}

export const accountApiClient = {
  getOverview(options: AccountApiRequestOptions = {}) {
    return request<AccountOverviewResponse>({
      method: accountApiEndpoints.overview.method,
      path: accountApiEndpoints.overview.path,
      ...options,
    });
  },

  getOptions(options: AccountApiRequestOptions = {}) {
    return request<AccountOptionsResponse>({
      method: accountApiEndpoints.optionsGet.method,
      path: accountApiEndpoints.optionsGet.path,
      ...options,
    });
  },

  getProfile(options: AccountApiRequestOptions = {}) {
    return request<MyProfileResponse>({
      method: accountApiEndpoints.profileGet.method,
      path: accountApiEndpoints.profileGet.path,
      ...options,
    });
  },

  updateProfile(input: UpdateMyProfileRequest, options: AccountApiRequestOptions = {}) {
    return request<MyProfileResponse>({
      method: accountApiEndpoints.profileUpdate.method,
      path: accountApiEndpoints.profileUpdate.path,
      body: input,
      ...options,
    });
  },

  uploadProfileAvatar(formData: FormData, options: AccountApiRequestOptions = {}) {
    return request<AvatarUploadResponse>({
      method: accountApiEndpoints.profileAvatarUpload.method,
      path: accountApiEndpoints.profileAvatarUpload.path,
      body: formData,
      ...options,
    });
  },

  removeProfileAvatar(options: AccountApiRequestOptions = {}) {
    return request<void>({
      method: accountApiEndpoints.profileAvatarDelete.method,
      path: accountApiEndpoints.profileAvatarDelete.path,
      ...options,
    });
  },

  getPreferences(options: AccountApiRequestOptions = {}) {
    return request<UserPreferenceResponse>({
      method: accountApiEndpoints.preferencesGet.method,
      path: accountApiEndpoints.preferencesGet.path,
      ...options,
    });
  },

  updatePreferences(input: UpdateUserPreferenceRequest, options: AccountApiRequestOptions = {}) {
    return request<UserPreferenceResponse>({
      method: accountApiEndpoints.preferencesUpdate.method,
      path: accountApiEndpoints.preferencesUpdate.path,
      body: input,
      ...options,
    });
  },

  uploadPreferencesFavicon(formData: FormData, options: AccountApiRequestOptions = {}) {
    return request<AvatarUploadResponse>({
      method: accountApiEndpoints.preferencesFaviconUpload.method,
      path: accountApiEndpoints.preferencesFaviconUpload.path,
      body: formData,
      ...options,
    });
  },

  removePreferencesFavicon(options: AccountApiRequestOptions = {}) {
    return request<void>({
      method: accountApiEndpoints.preferencesFaviconDelete.method,
      path: accountApiEndpoints.preferencesFaviconDelete.path,
      ...options,
    });
  },

  getSessions(options: AccountApiRequestOptions = {}) {
    return request<UserSessionsResponse>({
      method: accountApiEndpoints.sessionsGet.method,
      path: accountApiEndpoints.sessionsGet.path,
      ...options,
    });
  },

  revokeSession(sessionId: string, options: AccountApiRequestOptions = {}) {
    const endpoint = accountApiEndpoints.sessionsRevoke(sessionId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  revokeOtherSessions(options: AccountApiRequestOptions = {}) {
    return request<void>({
      method: accountApiEndpoints.sessionsRevokeOthers.method,
      path: accountApiEndpoints.sessionsRevokeOthers.path,
      ...options,
    });
  },

  getTrustedDevices(options: AccountApiRequestOptions = {}) {
    return request<TrustedDevicesResponse>({
      method: accountApiEndpoints.trustedDevicesGet.method,
      path: accountApiEndpoints.trustedDevicesGet.path,
      ...options,
    });
  },

  revokeTrustedDevice(deviceId: string, options: AccountApiRequestOptions = {}) {
    const endpoint = accountApiEndpoints.trustedDevicesRevoke(deviceId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  revokeOtherTrustedDevices(options: AccountApiRequestOptions = {}) {
    return request<void>({
      method: accountApiEndpoints.trustedDevicesRevokeOthers.method,
      path: accountApiEndpoints.trustedDevicesRevokeOthers.path,
      ...options,
    });
  },

  changePassword(input: ChangePasswordRequest, options: AccountApiRequestOptions = {}) {
    return request<void>({
      method: accountApiEndpoints.securityPasswordChange.method,
      path: accountApiEndpoints.securityPasswordChange.path,
      body: input,
      ...options,
    });
  },

  requestEmailChange(input: EmailChangeRequest, options: AccountApiRequestOptions = {}) {
    return request<EmailChangeRequestResponse>({
      method: accountApiEndpoints.securityEmailChangeRequest.method,
      path: accountApiEndpoints.securityEmailChangeRequest.path,
      body: input,
      ...options,
    });
  },

  confirmEmailChange(input: EmailChangeConfirmRequest, options: AccountApiRequestOptions = {}) {
    return request<EmailChangeConfirmResponse>({
      method: accountApiEndpoints.securityEmailChangeConfirm.method,
      path: accountApiEndpoints.securityEmailChangeConfirm.path,
      body: input,
      ...options,
    });
  },

  getMfaStatus(options: AccountApiRequestOptions = {}) {
    return request<MfaStatusResponse>({
      method: accountApiEndpoints.securityMfaGet.method,
      path: accountApiEndpoints.securityMfaGet.path,
      ...options,
    });
  },

  setupMfa(options: AccountApiRequestOptions = {}) {
    return request<MfaSetupResponse>({
      method: accountApiEndpoints.securityMfaSetup.method,
      path: accountApiEndpoints.securityMfaSetup.path,
      ...options,
    });
  },

  confirmMfa(input: ConfirmMfaRequest, options: AccountApiRequestOptions = {}) {
    return request<ConfirmMfaResponse>({
      method: accountApiEndpoints.securityMfaConfirm.method,
      path: accountApiEndpoints.securityMfaConfirm.path,
      body: input,
      ...options,
    });
  },

  disableMfa(input: DisableMfaRequest, options: AccountApiRequestOptions = {}) {
    return request<void>({
      method: accountApiEndpoints.securityMfaDisable.method,
      path: accountApiEndpoints.securityMfaDisable.path,
      body: input,
      ...options,
    });
  },

  regenerateMfaRecoveryCodes(options: AccountApiRequestOptions = {}) {
    return request<RecoveryCodesResponse>({
      method: accountApiEndpoints.securityMfaRecoveryRegenerate.method,
      path: accountApiEndpoints.securityMfaRecoveryRegenerate.path,
      ...options,
    });
  },

  getNotifications(filter: string | undefined, options: AccountApiRequestOptions = {}) {
    return request<AccountNotificationsResponse>({
      method: accountApiEndpoints.notificationsGet.method,
      path: accountApiEndpoints.notificationsGet.path,
      query: { filter },
      ...options,
    });
  },

  markNotificationAsRead(notificationId: string, options: AccountApiRequestOptions = {}) {
    const endpoint = accountApiEndpoints.notificationsMarkRead(notificationId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  markAllNotificationsAsRead(options: AccountApiRequestOptions = {}) {
    return request<void>({
      method: accountApiEndpoints.notificationsMarkAllRead.method,
      path: accountApiEndpoints.notificationsMarkAllRead.path,
      ...options,
    });
  },

  deleteNotification(notificationId: string, options: AccountApiRequestOptions = {}) {
    const endpoint = accountApiEndpoints.notificationsDelete(notificationId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getNotificationPreferences(options: AccountApiRequestOptions = {}) {
    return request<NotificationPreferencesResponse>({
      method: accountApiEndpoints.notificationPreferencesGet.method,
      path: accountApiEndpoints.notificationPreferencesGet.path,
      ...options,
    });
  },

  updateNotificationPreferences(
    input: UpdateNotificationPreferencesRequest,
    options: AccountApiRequestOptions = {},
  ) {
    return request<NotificationPreferencesResponse>({
      method: accountApiEndpoints.notificationPreferencesUpdate.method,
      path: accountApiEndpoints.notificationPreferencesUpdate.path,
      body: input,
      ...options,
    });
  },

  getAuditEntries(
    query: { limit?: number; eventType?: string } = {},
    options: AccountApiRequestOptions = {},
  ) {
    return request<AccountAuditEntriesResponse>({
      method: accountApiEndpoints.auditList.method,
      path: accountApiEndpoints.auditList.path,
      query,
      ...options,
    });
  },

  getConsents(options: AccountApiRequestOptions = {}) {
    return request<ConsentsResponse>({
      method: accountApiEndpoints.consentsGet.method,
      path: accountApiEndpoints.consentsGet.path,
      ...options,
    });
  },

  acceptConsent(
    consentType: string,
    input: AcceptConsentRequest,
    options: AccountApiRequestOptions = {},
  ) {
    const endpoint = accountApiEndpoints.consentsAccept(consentType);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  createInvitation(input: CreateAccountInvitationRequest, options: AccountApiRequestOptions = {}) {
    return request<AccountInvitationResponse>({
      method: accountApiEndpoints.invitationsCreate.method,
      path: accountApiEndpoints.invitationsCreate.path,
      body: input,
      ...options,
    });
  },

  listInvitations(options: AccountApiRequestOptions = {}) {
    return request<AccountInvitationSummaryResponse[]>({
      method: accountApiEndpoints.invitationsList.method,
      path: accountApiEndpoints.invitationsList.path,
      ...options,
    });
  },

  resendInvitation(invitationId: string, options: AccountApiRequestOptions = {}) {
    const endpoint = accountApiEndpoints.invitationsResend(invitationId);
    return request<AccountInvitationResponse>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  revokeInvitation(invitationId: string, options: AccountApiRequestOptions = {}) {
    const endpoint = accountApiEndpoints.invitationsRevoke(invitationId);
    return request<AccountInvitationResponse>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listMembers(options: AccountApiRequestOptions = {}) {
    return request<AccountMemberResponse[]>({
      method: accountApiEndpoints.membersList.method,
      path: accountApiEndpoints.membersList.path,
      ...options,
    });
  },

  listRoleCatalog(options: AccountApiRequestOptions = {}) {
    return request<AccountRoleCatalogResponse[]>({
      method: accountApiEndpoints.rolesCatalogList.method,
      path: accountApiEndpoints.rolesCatalogList.path,
      ...options,
    });
  },

  updateMemberRoles(
    userId: string,
    input: UpdateAccountMemberRolesRequest,
    options: AccountApiRequestOptions = {},
  ) {
    const endpoint = accountApiEndpoints.memberRolesUpdate(userId);
    return request<AccountMemberResponse>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },
};
