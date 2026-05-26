// <copyright file="IOmnichannelDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Omnichannel.Domain.Entities;

namespace NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;

public interface IOmnichannelDbContext
{
    DbSet<ChannelAccount> Accounts { get; }
    DbSet<ChannelConversation> Conversations { get; }
    DbSet<ChannelMessage> Messages { get; }
    DbSet<ConversationNote> Notes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
