// <copyright file="CreateChannelAccountCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Contracts.DTOs;
using NetMetric.CRM.Omnichannel.Domain.Entities;

namespace NetMetric.CRM.Omnichannel.Application.Commands.Accounts.CreateChannelAccount;

public sealed class CreateChannelAccountCommandHandler(IOmnichannelDbContext dbContext) : IRequestHandler<CreateChannelAccountCommand, ChannelAccountDto>
{
    public async Task<ChannelAccountDto> Handle(CreateChannelAccountCommand request, CancellationToken cancellationToken)
    {
        var entity = new ChannelAccount(request.Name, request.ChannelType, request.ExternalAccountId, request.SecretReference, request.RoutingKey);
        await dbContext.Accounts.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ChannelAccountDto(entity.Id, entity.Name, entity.ChannelType.ToString(), entity.ExternalAccountId, entity.RoutingKey, entity.IsActive);
    }
}
