// <copyright file="RevokeOtherSessionsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Features.Commands;
using NetMetric.Auth.Application.Records;
using NetMetric.Clock;

namespace NetMetric.Auth.Application.Features.Handlers;

public sealed class RevokeOtherSessionsCommandHandler(
    IUserSessionRepository userSessionRepository,
    IUserSessionStateValidator userSessionStateValidator,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IClock clock) : IRequestHandler<RevokeOtherSessionsCommand, Unit>
{
    public async Task<Unit> Handle(RevokeOtherSessionsCommand request, CancellationToken cancellationToken)
    {
        var revokedSessionIds = await userSessionRepository.RevokeAllAsync(
            request.TenantId,
            request.UserId,
            clock.UtcDateTime,
            "user_revoked_other_sessions",
            request.CurrentSessionId,
            cancellationToken);

        await auditTrail.WriteAsync(
            new AuthAuditRecord(
                request.TenantId,
                "auth.session.revoke-others",
                "success",
                request.UserId,
                request.CurrentSessionId,
                request.Email,
                request.IpAddress,
                request.UserAgent,
                request.CorrelationId,
                request.TraceId),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var sessionId in revokedSessionIds)
        {
            userSessionStateValidator.Evict(request.TenantId, sessionId);
        }

        return Unit.Value;
    }
}
