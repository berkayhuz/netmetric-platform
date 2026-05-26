// <copyright file="DataQualityIssueDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.DataQuality;

public sealed class DataQualityIssueDto
{
    public required string EntityType { get; init; }
    public required Guid EntityId { get; init; }
    public required string DisplayName { get; init; }
    public required string IssueCode { get; init; }
    public required string Message { get; init; }
    public int Severity { get; init; }
}
