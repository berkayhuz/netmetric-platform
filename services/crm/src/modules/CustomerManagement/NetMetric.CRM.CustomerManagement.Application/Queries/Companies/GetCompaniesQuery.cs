// <copyright file="GetCompaniesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Companies;

public sealed record GetCompaniesQuery(
    string? Search = null,
    CompanyType? CompanyType = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CompanyListItemDto>>;
