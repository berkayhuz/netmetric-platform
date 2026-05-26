// <copyright file="TagDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Tags;

public sealed class TagDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? ColorHex { get; init; }
    public string? Description { get; init; }
}
