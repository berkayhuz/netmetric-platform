// <copyright file="GetDealsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DealManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.DealManagement.Application.Queries.Deals;

public sealed record GetDealsQuery(string? Search, Guid? OwnerUserId, Guid? CompanyId, Guid? OpportunityId, bool? IsActive, int Page, int PageSize) : IRequest<PagedResult<DealListItemDto>>;
