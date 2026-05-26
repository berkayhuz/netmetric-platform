// <copyright file="IdentityServiceOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Infrastructure.Identity;

public sealed class IdentityServiceOptions
{
    public const string SectionName = "IdentityService";

    public string BaseUrl { get; init; } = string.Empty;
    public string SecuritySummaryPath { get; init; } = "/api/v1/internal/identity/users/{userId}/security-summary";
    public string ChangePasswordPath { get; init; } = "/api/v1/internal/identity/users/{userId}/password/change";
    public string MfaStatusPath { get; init; } = "/api/v1/internal/identity/users/{userId}/mfa";
    public string MfaSetupPath { get; init; } = "/api/v1/internal/identity/users/{userId}/mfa/setup";
    public string MfaConfirmPath { get; init; } = "/api/v1/internal/identity/users/{userId}/mfa/confirm";
    public string MfaDisablePath { get; init; } = "/api/v1/internal/identity/users/{userId}/mfa";
    public string RecoveryCodesRegeneratePath { get; init; } = "/api/v1/internal/identity/users/{userId}/mfa/recovery-codes/regenerate";
    public string EmailChangeRequestPath { get; init; } = "/api/v1/internal/identity/users/{userId}/email/change-request";
    public string EmailChangeConfirmPath { get; init; } = "/api/v1/internal/identity/users/{userId}/email/change-confirm";
    public string TrustedDevicesPath { get; init; } = "/api/v1/internal/identity/users/{userId}/trusted-devices";
    public string TrustedDeviceRevokePath { get; init; } = "/api/v1/internal/identity/users/{userId}/trusted-devices/{deviceId}";
    public string InvitationsPath { get; init; } = "/api/v1/internal/account-management/invitations";
    public string InvitationResendPath { get; init; } = "/api/v1/internal/account-management/invitations/{invitationId}/resend";
    public string InvitationRevokePath { get; init; } = "/api/v1/internal/account-management/invitations/{invitationId}/revoke";
    public string MembersPath { get; init; } = "/api/v1/internal/account-management/members";
    public string RolesCatalogPath { get; init; } = "/api/v1/internal/account-management/roles/catalog";
    public string MemberRolesPath { get; init; } = "/api/v1/internal/account-management/members/{userId}/roles";
    public int TimeoutSeconds { get; init; } = 10;
    public int GetRetryCount { get; init; } = 2;
    public int GetRetryDelayMilliseconds { get; init; } = 150;
}
