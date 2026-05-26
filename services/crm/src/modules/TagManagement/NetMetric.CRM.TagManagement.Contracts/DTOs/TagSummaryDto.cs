// <copyright file="TagSummaryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TagManagement.Contracts.DTOs;

public sealed class TagSummaryDto
{
    public required Guid TagId { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
    public required string? GroupName { get; init; }
}

public sealed class TagGroupSummaryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? EntityType { get; init; }
    public string? Color { get; init; }
}

public sealed class SmartLabelRuleSummaryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? EntityType { get; init; }
    public string? ConditionJson { get; init; }
}

public sealed class ClassificationSchemeSummaryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? EntityType { get; init; }
}
