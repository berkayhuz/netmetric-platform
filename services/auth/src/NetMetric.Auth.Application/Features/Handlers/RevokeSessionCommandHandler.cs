// <copyright file="RevokeSessionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Features.Commands;
using NetMetric.Auth.Application.Records;
using NetMetric.Clock;

namespace NetMetric.Auth.Application.Features.Handlers;

public sealed class RevokeSessionCommandHandler(
    IUserSessionRepository userSessionRepository,
    IUserSessionStateValidator userSessionStateValidator,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IClock clock) : IRequestHandler<RevokeSessionCommand, bool>
{
    public async Task<bool> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var revoked = await userSessionRepository.RevokeAsync(
            request.TenantId,
            request.UserId,
            request.SessionId,
            clock.UtcDateTime,
            request.SessionId == request.CurrentSessionId
                ? "user_revoked_current_session"
                : "user_revoked_session",
            cancellationToken);

        if (!revoked)
        {
            return false;
        }

        await auditTrail.WriteAsync(
            new AuthAuditRecord(
                request.TenantId,
                "auth.session.revoked",
                "success",
                request.UserId,
                request.SessionId,
                request.Email,
                request.IpAddress,
                request.UserAgent,
                request.CorrelationId,
                request.TraceId),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        userSessionStateValidator.Evict(request.TenantId, request.SessionId);
        return true;
    }
}
