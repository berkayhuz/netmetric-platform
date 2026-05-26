// <copyright file="SecurityEventContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Abstractions.Audit;

public sealed record SecurityEventWriteRequest(
    Guid TenantId,
    Guid UserId,
    string EventType,
    string Severity,
    string? CorrelationId,
    string? IpAddress,
    string? UserAgent,
    IReadOnlyDictionary<string, string>? Metadata);

public interface ISecurityEventWriter
{
    Task WriteAsync(SecurityEventWriteRequest request, CancellationToken cancellationToken = default);
}
