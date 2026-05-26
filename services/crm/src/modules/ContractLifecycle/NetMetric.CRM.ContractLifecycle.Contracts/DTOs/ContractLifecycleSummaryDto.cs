// <copyright file="ContractLifecycleSummaryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ContractLifecycle.Contracts.DTOs;

public sealed class ContractLifecycleSummaryDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }
}
