// <copyright file="CustomFieldDefinitionDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;

public sealed class CustomFieldDefinitionDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Label { get; init; }
    public required string EntityName { get; init; }
    public required string DataType { get; init; }
    public bool IsRequired { get; init; }
    public bool IsUnique { get; init; }
    public bool IsSystem { get; init; }
    public string? DefaultValue { get; init; }
    public string? Placeholder { get; init; }
    public string? HelpText { get; init; }
    public int OrderNo { get; init; }
    public IReadOnlyList<CustomFieldOptionDto> Options { get; init; } = [];
}

public sealed class CustomFieldOptionDto
{
    public required Guid Id { get; init; }
    public required string Label { get; init; }
    public required string Value { get; init; }
    public int OrderNo { get; init; }
}
