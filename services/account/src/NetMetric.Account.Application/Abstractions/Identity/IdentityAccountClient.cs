// <copyright file="IdentityAccountClient.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Abstractions.Identity;

public sealed record AccountSecuritySummary(bool IsMfaEnabled, DateTimeOffset? LastSecurityEventAt);

public sealed record PasswordPolicyFailure(string Code, string Message);

public sealed record ChangePasswordIdentityRequest(string CurrentPassword, string NewPassword, bool RevokeOtherSessions);

public sealed record ChangePasswordIdentityResult(bool Succeeded, IReadOnlyCollection<PasswordPolicyFailure> Failures);

public sealed record MfaStatusResult(bool IsEnabled, bool HasAuthenticator, int RecoveryCodesRemaining);

public sealed record MfaSetupResult(string SharedKey, string AuthenticatorUri);

public sealed record MfaConfirmResult(bool Succeeded, IReadOnlyCollection<string> RecoveryCodes);

public sealed record RecoveryCodesResult(IReadOnlyCollection<string> RecoveryCodes);

public sealed record EmailChangeRequestIdentityRequest(string NewEmail, string CurrentPassword);

public sealed record EmailChangeRequestIdentityResult(bool Succeeded, IReadOnlyCollection<PasswordPolicyFailure> Failures);

public sealed record EmailChangeConfirmIdentityRequest(string Token);

public sealed record EmailChangeConfirmIdentityResult(bool Succeeded, string? NewEmail, IReadOnlyCollection<PasswordPolicyFailure> Failures);

public sealed record TrustedDeviceIdentityResponse(
    Guid Id,
    bool IsCurrent,
    string? DeviceName,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset TrustedAt,
    DateTimeOffset? LastSeenAt,
    bool IsRevoked);

public sealed record TrustedDevicesIdentityResponse(IReadOnlyCollection<TrustedDeviceIdentityResponse> Items);

public sealed record CreateTenantInvitationIdentityRequest(
    string Email,
    string? FirstName,
    string? LastName);

public sealed record TenantInvitationIdentityResponse(
    Guid TenantId,
    Guid InvitationId,
    string Email,
    DateTime ExpiresAtUtc,
    string Status,
    DateTime? LastSentAtUtc);

public sealed record TenantInvitationSummaryIdentityResponse(
    Guid TenantId,
    Guid InvitationId,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime ExpiresAtUtc,
    string Status,
    int ResendCount,
    DateTime CreatedAtUtc,
    DateTime? LastSentAtUtc,
    DateTime? AcceptedAtUtc,
    DateTime? RevokedAtUtc,
    string? LastDeliveryStatus);

public sealed record TenantMemberIdentityResponse(
    Guid TenantId,
    Guid UserId,
    string UserName,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public sealed record RoleCatalogIdentityResponse(
    string Name,
    int Rank,
    bool IsProtected,
    IReadOnlyCollection<string> Permissions);

public sealed record UpdateTenantMemberRolesIdentityRequest(IReadOnlyCollection<string> Roles);

public sealed class IdentityServiceException(
    string message,
    string errorCode,
    int statusCode,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
}

public interface IIdentityAccountClient
{
    Task<AccountSecuritySummary> GetSecuritySummaryAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<ChangePasswordIdentityResult> ChangePasswordAsync(
        Guid tenantId,
        Guid userId,
        ChangePasswordIdentityRequest request,
        CancellationToken cancellationToken = default);

    Task<MfaStatusResult> GetMfaStatusAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<MfaSetupResult> StartMfaSetupAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<MfaConfirmResult> ConfirmMfaAsync(Guid tenantId, Guid userId, string verificationCode, CancellationToken cancellationToken = default);

    Task DisableMfaAsync(Guid tenantId, Guid userId, string verificationCode, CancellationToken cancellationToken = default);

    Task<RecoveryCodesResult> RegenerateRecoveryCodesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<EmailChangeRequestIdentityResult> RequestEmailChangeAsync(
        Guid tenantId,
        Guid userId,
        EmailChangeRequestIdentityRequest request,
        CancellationToken cancellationToken = default);

    Task<EmailChangeConfirmIdentityResult> ConfirmEmailChangeAsync(
        Guid tenantId,
        Guid userId,
        EmailChangeConfirmIdentityRequest request,
        CancellationToken cancellationToken = default);

    Task<TrustedDevicesIdentityResponse> GetTrustedDevicesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> RevokeTrustedDeviceAsync(Guid tenantId, Guid userId, Guid deviceId, CancellationToken cancellationToken = default);

    Task<TenantInvitationIdentityResponse> CreateInvitationAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTenantInvitationIdentityRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantInvitationSummaryIdentityResponse>> ListInvitationsAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default);

    Task<TenantInvitationIdentityResponse> ResendInvitationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid invitationId,
        CancellationToken cancellationToken = default);

    Task<TenantInvitationIdentityResponse> RevokeInvitationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid invitationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantMemberIdentityResponse>> ListMembersAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RoleCatalogIdentityResponse>> ListRoleCatalogAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default);

    Task<TenantMemberIdentityResponse> UpdateMemberRolesAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid targetUserId,
        UpdateTenantMemberRolesIdentityRequest request,
        CancellationToken cancellationToken = default);
}
