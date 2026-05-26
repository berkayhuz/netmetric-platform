// <copyright file="GetMySessionsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Sessions;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Sessions;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Sessions.Queries;

public sealed record GetMySessionsQuery : IRequest<Result<UserSessionsResponse>>;

public sealed class GetMySessionsQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserSession> sessions)
    : IRequestHandler<GetMySessionsQuery, Result<UserSessionsResponse>>
{
    public async Task<Result<UserSessionsResponse>> Handle(GetMySessionsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var sessionItems = await sessions.Query
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderByDescending(x => x.LastSeenAt)
            .ToListAsync(cancellationToken);

        var response = new UserSessionsResponse(sessionItems
            .Select(session => new UserSessionResponse(
                session.Id,
                session.DeviceName,
                session.IpAddress,
                session.UserAgent,
                session.ApproximateLocation,
                session.CreatedAt,
                session.LastSeenAt,
                session.ExpiresAt,
                currentUser.SessionId == session.Id,
                session.IsActive(clock.UtcNow)))
            .ToArray());

        return Result<UserSessionsResponse>.Success(response);
    }
}
