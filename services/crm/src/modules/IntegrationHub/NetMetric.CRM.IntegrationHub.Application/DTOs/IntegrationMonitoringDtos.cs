// <copyright file="IntegrationMonitoringDtos.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.IntegrationHub.Application.DTOs;

public sealed record IntegrationJobListItemDto(
    Guid Id,
    string ProviderKey,
    string JobType,
    string Direction,
    string Status,
    DateTime ScheduledAtUtc,
    DateTime? NextAttemptAtUtc,
    DateTime? CompletedAtUtc,
    int AttemptCount,
    int MaxAttempts,
    string? ErrorClassification,
    string? LastErrorCode,
    string? LastErrorMessage,
    bool IsReplay);

public sealed record IntegrationJobDetailDto(
    Guid Id,
    string ProviderKey,
    string JobType,
    string Direction,
    string Status,
    DateTime ScheduledAtUtc,
    DateTime? NextAttemptAtUtc,
    DateTime? StartedAtUtc,
    DateTime? LastAttemptAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? CancelledAtUtc,
    DateTime? DeadLetteredAtUtc,
    int AttemptCount,
    int MaxAttempts,
    string IdempotencyKey,
    string? ErrorClassification,
    string? LastErrorCode,
    string? LastErrorMessage,
    string? CorrelationId,
    Guid? ReplayOfJobId,
    IReadOnlyCollection<IntegrationJobExecutionLogDto> Executions);

public sealed record IntegrationJobExecutionLogDto(
    Guid Id,
    int AttemptNumber,
    string Status,
    string Message,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    string CorrelationId,
    string? ErrorClassification,
    string? ErrorCode);

public sealed record IntegrationDeadLetterDto(
    Guid Id,
    Guid JobId,
    string ProviderKey,
    string JobType,
    string Direction,
    int AttemptCount,
    string ErrorClassification,
    string ErrorCode,
    string SanitizedErrorMessage,
    DateTime FailedAtUtc,
    string Status,
    Guid? ReplayedJobId,
    DateTime? ReplayedAtUtc);

public sealed record IntegrationConnectorHealthDto(
    Guid ConnectionId,
    string ProviderKey,
    string DisplayName,
    string Category,
    bool IsEnabled,
    string HealthStatus,
    string? HealthMessage,
    DateTime? LastHealthCheckAtUtc,
    int SecretVersion,
    DateTime? SecretRotatedAtUtc);

public sealed record IntegrationWorkerStatusDto(
    bool IsEnabled,
    int DueJobs,
    int ProcessingJobs,
    int RetryingJobs,
    int DeadLetteredJobs,
    DateTime CheckedAtUtc);

public sealed record WebhookValidationResultDto(
    Guid DeliveryId,
    bool Accepted,
    bool Duplicate,
    string Status,
    string? FailureReason);
