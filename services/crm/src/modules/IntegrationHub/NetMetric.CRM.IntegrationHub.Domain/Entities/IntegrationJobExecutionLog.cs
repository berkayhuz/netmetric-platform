// <copyright file="IntegrationJobExecutionLog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class IntegrationJobExecutionLog : EntityBase
{
    public Guid JobId { get; private set; }
    public int AttemptNumber { get; private set; }
    public string Status { get; private set; } = null!;
    public string? ErrorClassification { get; private set; }
    public string? ErrorCode { get; private set; }
    public string Message { get; private set; } = null!;
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string CorrelationId { get; private set; } = null!;

    private IntegrationJobExecutionLog() { }

    public IntegrationJobExecutionLog(
        Guid tenantId,
        Guid jobId,
        int attemptNumber,
        string status,
        string message,
        DateTime startedAtUtc,
        DateTime? completedAtUtc,
        string correlationId,
        string? errorClassification = null,
        string? errorCode = null)
    {
        TenantId = tenantId;
        JobId = jobId;
        AttemptNumber = attemptNumber;
        Status = Guard.AgainstNullOrWhiteSpace(status).Trim();
        Message = Guard.AgainstNullOrWhiteSpace(message).Trim();
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId).Trim();
        ErrorClassification = string.IsNullOrWhiteSpace(errorClassification) ? null : errorClassification.Trim();
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? null : errorCode.Trim();
    }
}
