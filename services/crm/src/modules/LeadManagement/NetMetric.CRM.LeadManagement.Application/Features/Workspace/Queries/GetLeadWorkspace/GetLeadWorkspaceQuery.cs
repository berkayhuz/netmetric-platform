// <copyright file="GetLeadWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Features.Workspace.Queries.GetLeadWorkspace;

public sealed record GetLeadWorkspaceQuery(Guid LeadId) : IRequest<LeadWorkspaceDto?>;
