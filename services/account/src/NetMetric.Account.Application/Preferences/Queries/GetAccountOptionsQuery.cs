// <copyright file="GetAccountOptionsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Preferences;

namespace NetMetric.Account.Application.Preferences.Queries;

public sealed record GetAccountOptionsQuery : IRequest<Result<AccountOptionsResponse>>;

public sealed class GetAccountOptionsQueryHandler : IRequestHandler<GetAccountOptionsQuery, Result<AccountOptionsResponse>>
{
    public Task<Result<AccountOptionsResponse>> Handle(GetAccountOptionsQuery request, CancellationToken cancellationToken)
        => Task.FromResult(Result<AccountOptionsResponse>.Success(AccountOptionsCatalog.CreateResponse()));
}
