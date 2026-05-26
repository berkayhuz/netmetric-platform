// <copyright file="PersistentAccountAuditWriter.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Domain.Audit;
using NetMetric.Account.Domain.Common;
using NetMetric.Clock;

namespace NetMetric.Account.Infrastructure.Audit;

public sealed class PersistentAccountAuditWriter(
    IRepository<IAccountDbContext, AccountAuditEntry> auditEntries,
    IAccountDbContext dbContext,
    IClock clock,
    IAuditMetadataSanitizer metadataSanitizer,
    IOptions<AccountAuditOptions> options,
    ILogger<PersistentAccountAuditWriter> logger)
    : IAccountAuditWriter
{
    public async Task WriteAsync(AccountAuditWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (!options.Value.PersistAuditEntries)
        {
            return;
        }

        var sanitizedMetadata = metadataSanitizer.Sanitize(request.Metadata);
        var metadataJson = sanitizedMetadata is null ? null : JsonSerializer.Serialize(sanitizedMetadata);
        if (metadataJson?.Length > options.Value.MetadataMaxLength)
        {
            metadataJson = metadataJson[..options.Value.MetadataMaxLength];
        }

        var entry = AccountAuditEntry.Create(
            TenantId.From(request.TenantId),
            UserId.From(request.UserId),
            request.EventType,
            Enum.TryParse<AuditSeverity>(request.Severity, true, out var severity) ? severity : AuditSeverity.Information,
            clock.UtcNow,
            request.CorrelationId);

        entry.AttachRequestMetadata(
            request.IpAddress,
            request.UserAgent,
            metadataJson);

        await auditEntries.AddAsync(entry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "AUDIT_EVENT EventType={EventType} Severity={Severity} TenantId={TenantId} UserId={UserId} CorrelationId={CorrelationId}",
            request.EventType,
            request.Severity,
            request.TenantId,
            request.UserId,
            request.CorrelationId);
    }
}
