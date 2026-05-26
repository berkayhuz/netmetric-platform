// <copyright file="GetPipelineBoardQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Pipeline.Queries.GetPipelineBoard;

public sealed record GetPipelineBoardQuery(Guid? OwnerUserId, string? Search, int MaxItemsPerStage = 25) : IRequest<IReadOnlyList<PipelineColumnDto>>;
