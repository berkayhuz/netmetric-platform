// <copyright file="DocumentMetadataDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DocumentManagement.Contracts.DTOs;

public sealed class DocumentMetadataDto
{
    public required Guid DocumentId { get; init; }
    public required string Name { get; init; }
    public required string ContentType { get; init; }
    public required int VersionCount { get; init; }
    public required string? PreviewUrl { get; init; }
}
