// <copyright file="GetOmnichannelWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Contracts.DTOs;
using NetMetric.CRM.Omnichannel.Domain.Enums;

namespace NetMetric.CRM.Omnichannel.Application.Queries.GetOmnichannelWorkspace;

public sealed class GetOmnichannelWorkspaceQueryHandler(IOmnichannelDbContext dbContext) : IRequestHandler<GetOmnichannelWorkspaceQuery, OmnichannelWorkspaceDto>
{
    public async Task<OmnichannelWorkspaceDto> Handle(GetOmnichannelWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var accountRows = await dbContext.Accounts
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var accounts = accountRows
            .Select(x => new ChannelAccountDto(x.Id, x.Name, x.ChannelType.ToString(), x.ExternalAccountId, x.RoutingKey, x.IsActive))
            .ToList();

        var conversationRows = await dbContext.Conversations
            .OrderByDescending(x => x.LastMessageAtUtc)
            .Take(25)
            .ToListAsync(cancellationToken);

        var conversations = conversationRows
            .Select(x => new ChannelConversationDto(x.Id, x.AccountId, x.Subject, x.CustomerDisplayName, x.Status.ToString(), x.LastMessageAtUtc))
            .ToList();

        var openConversationCount = await dbContext.Conversations.CountAsync(x => x.Status == ConversationStatus.Open || x.Status == ConversationStatus.Pending, cancellationToken);

        return new OmnichannelWorkspaceDto(accounts, conversations, openConversationCount);
    }
}
