// <copyright file="ManagementController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetMetric.Account.Api.DependencyInjection;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Contracts.Management;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account")]
public sealed class ManagementController(
    IIdentityAccountClient identityAccountClient,
    ICurrentUserAccessor currentUserAccessor,
    IAccountAuditWriter auditWriter) : ControllerBase
{
    [HttpPost("invitations")]
    [Authorize(Policy = AccountPolicies.UsersInvite)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<AccountInvitationResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountInvitationResponse>> CreateInvitation(
        [FromBody] CreateAccountInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var response = await identityAccountClient.CreateInvitationAsync(
            currentUser.TenantId,
            currentUser.UserId,
            new CreateTenantInvitationIdentityRequest(request.Email, request.FirstName, request.LastName),
            cancellationToken);
        await WriteAuditAsync(
            currentUser,
            AccountAuditEventTypes.InvitationCreated,
            "Warning",
            new Dictionary<string, string>
            {
                ["targetEmail"] = request.Email,
                ["invitationId"] = response.InvitationId.ToString("D")
            },
            cancellationToken);

        return Ok(Map(response));
    }

    [HttpGet("invitations")]
    [Authorize(Policy = AccountPolicies.UsersInvite)]
    [ProducesResponseType<IReadOnlyCollection<AccountInvitationSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AccountInvitationSummaryResponse>>> ListInvitations(CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var response = await identityAccountClient.ListInvitationsAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        return Ok(response.Select(Map).ToArray());
    }

    [HttpPost("invitations/{invitationId:guid}/resend")]
    [Authorize(Policy = AccountPolicies.UsersInvite)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<AccountInvitationResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountInvitationResponse>> ResendInvitation(Guid invitationId, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var response = await identityAccountClient.ResendInvitationAsync(
            currentUser.TenantId,
            currentUser.UserId,
            invitationId,
            cancellationToken);
        await WriteAuditAsync(
            currentUser,
            AccountAuditEventTypes.InvitationResent,
            "Warning",
            new Dictionary<string, string>
            {
                ["targetEmail"] = response.Email,
                ["invitationId"] = response.InvitationId.ToString("D")
            },
            cancellationToken);

        return Ok(Map(response));
    }

    [HttpPost("invitations/{invitationId:guid}/revoke")]
    [Authorize(Policy = AccountPolicies.UsersInvite)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<AccountInvitationResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountInvitationResponse>> RevokeInvitation(Guid invitationId, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var response = await identityAccountClient.RevokeInvitationAsync(
            currentUser.TenantId,
            currentUser.UserId,
            invitationId,
            cancellationToken);
        await WriteAuditAsync(
            currentUser,
            AccountAuditEventTypes.InvitationRevoked,
            "Warning",
            new Dictionary<string, string>
            {
                ["targetEmail"] = response.Email,
                ["invitationId"] = response.InvitationId.ToString("D")
            },
            cancellationToken);

        return Ok(Map(response));
    }

    [HttpGet("members")]
    [Authorize(Policy = AccountPolicies.RolesRead)]
    [ProducesResponseType<IReadOnlyCollection<AccountMemberResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AccountMemberResponse>>> ListMembers(CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var response = await identityAccountClient.ListMembersAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        return Ok(response.Select(Map).ToArray());
    }

    [HttpGet("roles/catalog")]
    [Authorize(Policy = AccountPolicies.RolesRead)]
    [ProducesResponseType<IReadOnlyCollection<AccountRoleCatalogResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AccountRoleCatalogResponse>>> ListRoleCatalog(CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var response = await identityAccountClient.ListRoleCatalogAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        return Ok(response.Select(Map).ToArray());
    }

    [HttpPut("members/{userId:guid}/roles")]
    [Authorize(Policy = AccountPolicies.RolesManage)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType<AccountMemberResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountMemberResponse>> UpdateMemberRoles(
        Guid userId,
        [FromBody] UpdateAccountMemberRolesRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var response = await identityAccountClient.UpdateMemberRolesAsync(
            currentUser.TenantId,
            currentUser.UserId,
            userId,
            new UpdateTenantMemberRolesIdentityRequest(request.Roles),
            cancellationToken);
        await WriteAuditAsync(
            currentUser,
            AccountAuditEventTypes.RoleChanged,
            "Critical",
            new Dictionary<string, string>
            {
                ["targetUserId"] = userId.ToString("D"),
                ["roles"] = string.Join(",", request.Roles.Order(StringComparer.OrdinalIgnoreCase))
            },
            cancellationToken);

        return Ok(Map(response));
    }

    private Task WriteAuditAsync(
        CurrentUser currentUser,
        string eventType,
        string severity,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken)
        => auditWriter.WriteAsync(
            new AccountAuditWriteRequest(
                currentUser.TenantId,
                currentUser.UserId,
                eventType,
                severity,
                currentUser.CorrelationId,
                currentUser.IpAddress,
                currentUser.UserAgent,
                metadata),
            cancellationToken);

    private static AccountInvitationResponse Map(TenantInvitationIdentityResponse response)
        => new(response.TenantId, response.InvitationId, response.Email, response.ExpiresAtUtc, response.Status, response.LastSentAtUtc);

    private static AccountInvitationSummaryResponse Map(TenantInvitationSummaryIdentityResponse response)
        => new(
            response.TenantId,
            response.InvitationId,
            response.Email,
            response.FirstName,
            response.LastName,
            response.ExpiresAtUtc,
            response.Status,
            response.ResendCount,
            response.CreatedAtUtc,
            response.LastSentAtUtc,
            response.AcceptedAtUtc,
            response.RevokedAtUtc,
            response.LastDeliveryStatus);

    private static AccountMemberResponse Map(TenantMemberIdentityResponse response)
        => new(
            response.TenantId,
            response.UserId,
            response.UserName,
            response.Email,
            response.FirstName,
            response.LastName,
            response.IsActive,
            response.Roles,
            response.Permissions,
            response.CreatedAt,
            response.LastLoginAt);

    private static AccountRoleCatalogResponse Map(RoleCatalogIdentityResponse response)
        => new(response.Name, response.Rank, response.IsProtected, response.Permissions);
}
