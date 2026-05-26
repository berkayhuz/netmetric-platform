// <copyright file="GetOpportunityWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Workspace.Queries.GetOpportunityWorkspace;

public sealed record GetOpportunityWorkspaceQuery(Guid OpportunityId) : IRequest<OpportunityWorkspaceDto?>;
