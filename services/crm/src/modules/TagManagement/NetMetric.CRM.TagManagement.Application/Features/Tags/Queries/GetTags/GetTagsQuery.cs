// <copyright file="GetTagsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.Tags.Queries.GetTags;

public sealed record GetTagsQuery : IRequest<IReadOnlyList<TagSummaryDto>>;
