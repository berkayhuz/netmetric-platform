// <copyright file="GetIntegrationOverviewQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.GetIntegrationOverview;

public sealed record GetIntegrationOverviewQuery(Guid TenantId) : IRequest<IntegrationOverviewDto>;

public sealed record IntegrationOverviewDto(
    IReadOnlyCollection<IntegrationConnectionDto> Connections,
    IReadOnlyCollection<IntegrationJobDto> Jobs,
    IReadOnlyCollection<IntegrationLogDto> Logs,
    IReadOnlyCollection<WebhookDto> Webhooks);

public sealed record IntegrationConnectionDto(Guid Id, string ProviderKey, string DisplayName, string Category, bool IsEnabled, string HealthStatus, DateTime? LastHealthCheckAtUtc);
public sealed record IntegrationJobDto(Guid Id, string ProviderKey, string JobType, string Direction, string Status, DateTime ScheduledAtUtc, DateTime? CompletedAtUtc, int AttemptCount);
public sealed record IntegrationLogDto(Guid Id, string ProviderKey, string Direction, string Status, string Message, int RetryCount);
public sealed record WebhookDto(Guid Id, string EventKey, string TargetUrl, bool IsEnabled);
