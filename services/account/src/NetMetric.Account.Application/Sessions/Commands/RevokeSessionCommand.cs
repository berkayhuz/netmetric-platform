// <copyright file="RevokeSessionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Sessions;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Sessions.Commands;

public sealed record RevokeSessionCommand(Guid SessionId) : IRequest<Result>;

public sealed class RevokeSessionCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserSession> sessions,
    IAccountDbContext dbContext,
    IAccountAuditWriter auditWriter,
    IAccountOutboxWriter outboxWriter)
    : IRequestHandler<RevokeSessionCommand, Result>
{
    public async Task<Result> Handle(RevokeSessionCommand command, CancellationToken cancellationToken)
    {
        if (command.SessionId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Session id is required."));
        }

        var currentUser = currentUserAccessor.GetRequired();
        if (currentUser.SessionId == command.SessionId)
        {
            return Result.Failure(Error.Validation("Current active session cannot be revoked from this endpoint."));
        }

        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var session = await sessions.Query
            .FirstOrDefaultAsync(
                x => x.Id == command.SessionId && x.TenantId == tenantId && x.UserId == userId,
                cancellationToken);

        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session"));
        }

        session.Revoke(currentUser.SessionId == command.SessionId ? "self_revoked" : "user_revoked", clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        await outboxWriter.EnqueueAsync(
            currentUser.TenantId,
            AccountOutboxEventTypes.SessionRevoked,
            new AccountSessionRevokedEvent(
                1,
                currentUser.TenantId,
                currentUser.UserId,
                currentUser.CorrelationId,
                clock.UtcNow,
                command.SessionId,
                "user_revoked"),
            currentUser.CorrelationId,
            cancellationToken);

        await auditWriter.WriteAsync(
            new AccountAuditWriteRequest(
                currentUser.TenantId,
                currentUser.UserId,
                AccountAuditEventTypes.SessionRevoked,
                "Warning",
                currentUser.CorrelationId,
                currentUser.IpAddress,
                currentUser.UserAgent,
                new Dictionary<string, string> { ["sessionId"] = command.SessionId.ToString("D") }),
            cancellationToken);

        return Result.Success();
    }
}
