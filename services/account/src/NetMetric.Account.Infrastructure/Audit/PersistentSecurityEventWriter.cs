// <copyright file="PersistentSecurityEventWriter.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Infrastructure.Outbox;

namespace NetMetric.Account.Infrastructure.Audit;

public sealed class PersistentSecurityEventWriter(IAccountAuditWriter auditWriter, IAccountOutboxWriter outboxWriter) : ISecurityEventWriter
{
    public async Task WriteAsync(SecurityEventWriteRequest request, CancellationToken cancellationToken = default)
    {
        await auditWriter.WriteAsync(
            new AccountAuditWriteRequest(
                request.TenantId,
                request.UserId,
                request.EventType,
                request.Severity,
                request.CorrelationId,
                request.IpAddress,
                request.UserAgent,
                request.Metadata),
            cancellationToken);

        await outboxWriter.EnqueueAsync(request.TenantId, OutboxEventTypes.SecurityEventRaised, request, request.CorrelationId, cancellationToken);
    }
}
