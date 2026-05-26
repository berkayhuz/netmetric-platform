// <copyright file="AuthTestDataBuilder.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Application.Descriptors;
using NetMetric.Auth.Contracts.Responses;
using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.TestKit.Builders;

public static class AuthTestDataBuilder
{
    public static UserBuilder User() => new();

    public static LoginRequestBuilder LoginRequest() => new();

    public static RegisterRequestBuilder RegisterRequest() => new();

    public static AuthenticationTokenResponse TokenResponse(
        Guid? tenantId = null,
        Guid? userId = null,
        Guid? sessionId = null)
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new AuthenticationTokenResponse(
            "test-only-placeholder",
            now.AddMinutes(15),
            "test-only-placeholder",
            now.AddDays(14),
            tenantId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            "jane.doe",
            "jane.doe@example.com",
            sessionId ?? Guid.NewGuid());
    }

    private const string DescriptorPlaceholderValue = "example-value-for-tests";

    public static AccessTokenDescriptor AccessTokenDescriptor(string? value = null) =>
        new(
            value ?? DescriptorPlaceholderValue,
            new DateTime(2026, 1, 1, 0, 15, 0, DateTimeKind.Utc));

    public static RefreshTokenDescriptor RefreshTokenDescriptor(string? value = null, string? digest = null) =>
        new(
            value ?? DescriptorPlaceholderValue,
            digest ?? DescriptorPlaceholderValue,
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    public static Tenant Tenant(Guid? tenantId = null, bool isActive = true)
    {
        var id = tenantId ?? Guid.Parse("11111111-1111-1111-1111-111111111111");
        var tenant = new Tenant
        {
            Id = id,
            Name = "Test Tenant",
            Slug = $"test-{id:N}"[..13],
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        tenant.IsActive = isActive;
        return tenant;
    }

    public static UserTenantMembership Membership(
        Guid tenantId,
        Guid userId,
        string roles = "tenant-user",
        string permissions = "session:self,profile:self",
        bool isActive = true)
    {
        var membership = new UserTenantMembership
        {
            TenantId = tenantId,
            UserId = userId,
            Roles = roles,
            Permissions = permissions,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        };
        membership.SetActive(isActive);
        return membership;
    }

    public static TenantInvitation Invitation(
        Guid tenantId,
        Guid invitedByUserId,
        string email = "invitee@example.com",
        string tokenHash = "invite-token-hash",
        DateTime? expiresAtUtc = null) =>
        new()
        {
            TenantId = tenantId,
            InvitedByUserId = invitedByUserId,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc ?? new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
}
