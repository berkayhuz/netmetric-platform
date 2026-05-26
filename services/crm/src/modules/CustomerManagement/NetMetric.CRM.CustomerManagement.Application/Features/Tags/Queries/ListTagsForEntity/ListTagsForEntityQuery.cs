// <copyright file="ListTagsForEntityQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Tags;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Queries.ListTagsForEntity;

public sealed class ListTagsForEntityQuery : IRequest<IReadOnlyList<TagDto>>
{
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
}
