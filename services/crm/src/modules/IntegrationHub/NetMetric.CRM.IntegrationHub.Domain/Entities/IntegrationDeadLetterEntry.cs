// <copyright file="IntegrationDeadLetterEntry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class IntegrationDeadLetterEntry : EntityBase
{
    public Guid JobId { get; private set; }
    public string ProviderKey { get; private set; } = null!;
    public string JobType { get; private set; } = null!;
    public string Direction { get; private set; } = null!;
    public string IdempotencyKey { get; private set; } = null!;
    public string PayloadJson { get; private set; } = "{}";
    public int AttemptCount { get; private set; }
    public string ErrorClassification { get; private set; } = null!;
    public string ErrorCode { get; private set; } = null!;
    public string SanitizedErrorMessage { get; private set; } = null!;
    public DateTime FailedAtUtc { get; private set; }
    public DateTime? ReplayedAtUtc { get; private set; }
    public Guid? ReplayedJobId { get; private set; }
    public string Status { get; private set; } = IntegrationDeadLetterStatuses.Open;

    private IntegrationDeadLetterEntry() { }

    public IntegrationDeadLetterEntry(
        IntegrationJob job,
        string errorClassification,
        string errorCode,
        string sanitizedErrorMessage,
        DateTime failedAtUtc)
    {
        TenantId = job.TenantId;
        JobId = job.Id;
        ProviderKey = job.ProviderKey;
        JobType = job.JobType;
        Direction = job.Direction;
        IdempotencyKey = job.IdempotencyKey;
        PayloadJson = job.PayloadJson;
        AttemptCount = job.AttemptCount;
        ErrorClassification = Guard.AgainstNullOrWhiteSpace(errorClassification).Trim();
        ErrorCode = Guard.AgainstNullOrWhiteSpace(errorCode).Trim();
        SanitizedErrorMessage = Guard.AgainstNullOrWhiteSpace(sanitizedErrorMessage).Trim();
        FailedAtUtc = failedAtUtc;
    }

    public void MarkReplayed(Guid replayedJobId, DateTime replayedAtUtc)
    {
        ReplayedJobId = replayedJobId;
        ReplayedAtUtc = replayedAtUtc;
        Status = IntegrationDeadLetterStatuses.Replayed;
    }
}

public static class IntegrationDeadLetterStatuses
{
    public const string Open = "open";
    public const string Replayed = "replayed";
}
