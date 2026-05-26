// <copyright file="RevokeOtherSessionsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Sessions;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Sessions.Commands;

public sealed record RevokeOtherSessionsCommand : IRequest<Result>;

public sealed class RevokeOtherSessionsCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IReauthenticationService reauthenticationService,
    IRepository<IAccountDbContext, UserSession> sessions,
    IAccountDbContext dbContext,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter)
    : IRequestHandler<RevokeOtherSessionsCommand, Result>
{
    public async Task<Result> Handle(RevokeOtherSessionsCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(
            currentUser,
            new ReauthenticationRequirement(ReauthenticationOperations.RevokeOtherSessions, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));

        if (reauth.IsFailure)
        {
            return reauth;
        }

        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var sessionsToRevoke = await sessions.Query
            .Where(x =>
                x.TenantId == tenantId &&
                x.UserId == userId &&
                x.Id != currentUser.SessionId &&
                x.RevokedAt == null &&
                x.ExpiresAt > clock.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var session in sessionsToRevoke)
        {
            session.Revoke("revoke_other_sessions", clock.UtcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var metadata = new Dictionary<string, string> { ["revokedCount"] = sessionsToRevoke.Count.ToString() };
        await auditWriter.WriteAsync(CreateAudit(currentUser, AccountAuditEventTypes.OtherSessionsRevoked, "Warning", metadata), cancellationToken);
        await securityEventWriter.WriteAsync(CreateSecurityEvent(currentUser, AccountAuditEventTypes.OtherSessionsRevoked, "Warning", metadata), cancellationToken);

        return Result.Success();
    }

    private static AccountAuditWriteRequest CreateAudit(CurrentUser user, string eventType, string severity, IReadOnlyDictionary<string, string> metadata)
        => new(user.TenantId, user.UserId, eventType, severity, user.CorrelationId, user.IpAddress, user.UserAgent, metadata);

    private static SecurityEventWriteRequest CreateSecurityEvent(CurrentUser user, string eventType, string severity, IReadOnlyDictionary<string, string> metadata)
        => new(user.TenantId, user.UserId, eventType, severity, user.CorrelationId, user.IpAddress, user.UserAgent, metadata);
}
