// <copyright file="GetMyNotificationsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Notifications;
using NetMetric.Account.Domain.Audit;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Notifications;

namespace NetMetric.Account.Application.Notifications.Queries;

public sealed record GetMyNotificationsQuery(string? Filter) : IRequest<Result<AccountNotificationsResponse>>;

public sealed class GetMyNotificationsQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IRepository<IAccountDbContext, AccountAuditEntry> auditEntries,
    IRepository<IAccountDbContext, UserNotificationState> notificationStates)
    : IRequestHandler<GetMyNotificationsQuery, Result<AccountNotificationsResponse>>
{
    public async Task<Result<AccountNotificationsResponse>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);
        var filter = string.IsNullOrWhiteSpace(request.Filter) ? "all" : request.Filter.Trim().ToLowerInvariant();

        var latestAudit = await auditEntries.Query
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderByDescending(x => x.OccurredAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        var userFacingAudit = latestAudit
            .Where(x => IsUserFacingNotificationEvent(x.EventType))
            .ToArray();

        var ids = userFacingAudit.Select(x => x.Id).ToArray();
        var states = ids.Length == 0
            ? []
            : await notificationStates.Query
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.UserId == userId && ids.Contains(x.NotificationId))
                .ToListAsync(cancellationToken);
        var byId = states.ToDictionary(x => x.NotificationId);

        var items = userFacingAudit
            .Select(x =>
            {
                byId.TryGetValue(x.Id, out var state);
                return new
                {
                    Notification = new AccountNotificationResponse(
                        x.Id,
                        ToFriendlyEventTitle(x.EventType),
                        $"Recorded as {x.EventType}. Correlation: {x.CorrelationId ?? "not available"}.",
                        ToCategory(x.EventType),
                        NormalizeSeverity(x.Severity.ToString()),
                        x.OccurredAt,
                        state?.IsRead ?? false),
                    IsDeleted = state?.IsDeleted ?? false
                };
            })
            .Where(x => !x.IsDeleted)
            .Select(x => x.Notification)
            .ToArray();

        var filtered = items.Where(x => filter switch
        {
            "read" => x.IsRead,
            "unread" => !x.IsRead,
            _ => true
        }).ToArray();

        var result = new AccountNotificationsResponse(
            filtered,
            items.Length,
            items.Count(x => !x.IsRead),
            items.Count(x => x.IsRead));

        return Result<AccountNotificationsResponse>.Success(result);
    }

    private static bool IsUserFacingNotificationEvent(string? eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return false;
        }

        return eventType switch
        {
            AccountAuditEventTypes.SessionRevoked => true,
            AccountAuditEventTypes.OtherSessionsRevoked => true,
            AccountAuditEventTypes.PasswordChanged => true,
            AccountAuditEventTypes.MfaSetupStarted => true,
            AccountAuditEventTypes.MfaEnabled => true,
            AccountAuditEventTypes.MfaDisabled => true,
            AccountAuditEventTypes.RecoveryCodesRegenerated => true,
            AccountAuditEventTypes.TrustedDeviceRevoked => true,
            AccountAuditEventTypes.EmailChangeRequested => true,
            AccountAuditEventTypes.EmailChanged => true,
            AccountAuditEventTypes.ConsentAccepted => true,
            AccountAuditEventTypes.InvitationCreated => true,
            AccountAuditEventTypes.InvitationResent => true,
            AccountAuditEventTypes.InvitationRevoked => true,
            AccountAuditEventTypes.RoleChanged => true,
            _ => false
        };
    }

    private static string ToFriendlyEventTitle(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return "Account update";
        }

        var parts = eventType.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return "Account update";
        }

        return string.Join(' ', parts.Select(Capitalize));
    }

    private static string Capitalize(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];

    private static string NormalizeSeverity(string severity)
        => severity.Equals("error", StringComparison.OrdinalIgnoreCase) ? "Critical"
            : severity.Equals("warning", StringComparison.OrdinalIgnoreCase) ? "Warning"
            : "Info";

    private static string ToCategory(string eventType)
    {
        if (eventType.Contains("mfa", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("session", StringComparison.OrdinalIgnoreCase))
        {
            return "Security";
        }

        if (eventType.Contains("invite", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("member", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("role", StringComparison.OrdinalIgnoreCase))
        {
            return "Workspace";
        }

        if (eventType.Contains("profile", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("preference", StringComparison.OrdinalIgnoreCase))
        {
            return "Account";
        }

        return "System";
    }
}
