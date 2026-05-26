// <copyright file="GetLeadTimelineQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Features.Timeline.Queries.GetLeadTimeline;

public sealed record GetLeadTimelineQuery(Guid LeadId) : IRequest<IReadOnlyList<LeadTimelineEventDto>>;
