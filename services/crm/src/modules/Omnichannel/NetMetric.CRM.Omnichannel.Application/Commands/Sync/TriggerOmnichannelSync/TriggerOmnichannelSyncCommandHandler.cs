// <copyright file="TriggerOmnichannelSyncCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Contracts.DTOs;
using NetMetric.Exceptions;

namespace NetMetric.CRM.Omnichannel.Application.Commands.Sync.TriggerOmnichannelSync;

public sealed class TriggerOmnichannelSyncCommandHandler(IOmnichannelDbContext dbContext) : IRequestHandler<TriggerOmnichannelSyncCommand, ChannelAccountDto>
{
    public async Task<ChannelAccountDto> Handle(TriggerOmnichannelSyncCommand request, CancellationToken cancellationToken)
    {
        var accountExists = await dbContext.Accounts.AnyAsync(x => x.Id == request.AccountId, cancellationToken);
        if (!accountExists)
        {
            throw new InvalidOperationException($"Omnichannel account '{request.AccountId}' was not found.");
        }

        throw new ForbiddenAppException("Omnichannel sync is disabled until a production channel provider adapter and worker are configured.");
    }
}
