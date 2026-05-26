// <copyright file="AuditLog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Auditing;

public class AuditLog : AuditableEntity
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityName
    {
        get => EntityType;
        set => EntityType = value;
    }
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActionType
    {
        get => Action;
        set => Action = value;
    }
    public string? ChangesJson { get; set; }
    public string? ChangedColumnsJson
    {
        get => ChangesJson;
        set => ChangesJson = value;
    }
    public string? CorrelationId { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public DateTime ChangedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }
}
