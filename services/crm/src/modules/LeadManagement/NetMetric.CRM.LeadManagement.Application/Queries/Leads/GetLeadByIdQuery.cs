// <copyright file="GetLeadByIdQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Queries.Leads;

public sealed record GetLeadByIdQuery(Guid LeadId) : IRequest<LeadDetailDto?>;
