// <copyright file="IntegrationConnectorAbstractions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.IntegrationHub.Domain.Entities;

namespace NetMetric.CRM.IntegrationHub.Application.Abstractions.Connectors;

public interface IIntegrationConnector
{
    string ProviderKey { get; }

    Task<IntegrationConnectorHealthResult> CheckHealthAsync(
        IntegrationConnection connection,
        CancellationToken cancellationToken);

    Task<IntegrationJobExecutionResult> ExecuteAsync(
        IntegrationJobExecutionContext context,
        CancellationToken cancellationToken);
}

public interface IIntegrationConnectorRegistry
{
    IIntegrationConnector? Resolve(string providerKey);
}

public sealed record IntegrationJobExecutionContext(
    Guid TenantId,
    Guid JobId,
    string ProviderKey,
    string JobType,
    string Direction,
    string PayloadJson,
    string? DeltaSyncToken,
    string CorrelationId);

public sealed record IntegrationJobExecutionResult(
    bool Succeeded,
    string Message,
    string? DeltaSyncToken = null);

public sealed record IntegrationConnectorHealthResult(
    string Status,
    string? Message,
    DateTime CheckedAtUtc);

public sealed class IntegrationTransientException(string message, string errorCode = "transient_error")
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

public sealed class IntegrationRateLimitedException(string message, TimeSpan? retryAfter = null, string errorCode = "rate_limited")
    : Exception(message)
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
    public string ErrorCode { get; } = errorCode;
}

public sealed class IntegrationPermanentException(string message, string errorCode = "permanent_error")
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
