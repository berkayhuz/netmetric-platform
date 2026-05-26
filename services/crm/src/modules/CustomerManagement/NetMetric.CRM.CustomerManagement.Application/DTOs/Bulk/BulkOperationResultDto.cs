// <copyright file="BulkOperationResultDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Bulk;

public sealed class BulkOperationResultDto
{
    public required int RequestedCount { get; init; }
    public required int AffectedCount { get; init; }
    public required IReadOnlyList<Guid> MissingIds { get; init; }
    public string? Message { get; init; }
}
