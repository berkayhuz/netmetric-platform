// <copyright file="AuthAuditTrail.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Logging;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Records;
using NetMetric.Auth.Domain.Entities;
using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.Infrastructure.Services;

public sealed class AuthAuditTrail(AuthDbContext dbContext, ILogger<AuthAuditTrail> logger) : IAuthAuditTrail
{
    public async Task WriteAsync(AuthAuditRecord record, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "AUDIT_EVENT EventType={EventType} Outcome={Outcome} TenantId={TenantId} UserId={UserId} CorrelationId={CorrelationId}",
            record.EventType,
            record.Outcome,
            record.TenantId,
            record.UserId,
            record.CorrelationId);

        await dbContext.AuthAuditEvents.AddAsync(new AuthAuditEvent
        {
            TenantId = record.TenantId,
            UserId = record.UserId,
            SessionId = record.SessionId,
            EventType = record.EventType,
            Outcome = record.Outcome,
            Identity = MaskIdentity(record.Identity),
            IpAddress = record.IpAddress,
            UserAgent = record.UserAgent,
            CorrelationId = record.CorrelationId,
            TraceId = record.TraceId,
            Metadata = record.Metadata
        }, cancellationToken);
    }

    private static string? MaskIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var index = value.IndexOf('@');
        if (index <= 1)
        {
            return "***";
        }

        return $"{value[0]}***{value[index..]}";
    }
}
