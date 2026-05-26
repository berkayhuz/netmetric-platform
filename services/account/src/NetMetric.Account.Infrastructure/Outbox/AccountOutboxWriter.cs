// <copyright file="AccountOutboxWriter.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Outbox;
using NetMetric.Clock;

namespace NetMetric.Account.Infrastructure.Outbox;

public sealed class AccountOutboxWriter(
    IRepository<IAccountDbContext, AccountOutboxMessage> outboxMessages,
    IAccountDbContext dbContext,
    IClock clock)
    : IAccountOutboxWriter
{
    public async Task EnqueueAsync<TPayload>(
        Guid tenantId,
        string type,
        TPayload payload,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload);
        var message = AccountOutboxMessage.Create(TenantId.From(tenantId), type, json, clock.UtcNow, correlationId);

        await outboxMessages.AddAsync(message, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
