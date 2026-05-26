// <copyright file="GetLostReasonsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;

namespace NetMetric.CRM.PipelineManagement.Application.Queries;

public sealed record GetLostReasonsQuery() : IRequest<IReadOnlyList<LostReasonDto>>;
