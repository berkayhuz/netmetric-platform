// <copyright file="GetMyConsentsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Consents;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Consents;

namespace NetMetric.Account.Application.Consents.Queries;

public sealed record GetMyConsentsQuery : IRequest<Result<ConsentsResponse>>;

public sealed class GetMyConsentsQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IRepository<IAccountDbContext, UserConsent> consents)
    : IRequestHandler<GetMyConsentsQuery, Result<ConsentsResponse>>
{
    public async Task<Result<ConsentsResponse>> Handle(GetMyConsentsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var items = await consents.Query
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderByDescending(x => x.DecidedAt)
            .Select(x => new ConsentHistoryItemResponse(x.Id, x.ConsentType, x.Version, x.Status.ToString(), x.DecidedAt))
            .ToListAsync(cancellationToken);

        return Result<ConsentsResponse>.Success(new ConsentsResponse(items));
    }
}
