// <copyright file="NoteDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Notes;

public sealed class NoteDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public bool IsPinned { get; init; }
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
