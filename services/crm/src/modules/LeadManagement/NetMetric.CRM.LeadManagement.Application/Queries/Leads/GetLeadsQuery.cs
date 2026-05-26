// <copyright file="GetLeadsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.LeadManagement.Application.Queries.Leads;

public sealed record GetLeadsQuery(
    string? Search = null,
    LeadStatusType? Status = null,
    LeadSourceType? Source = null,
    Guid? OwnerUserId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<LeadListItemDto>>;
