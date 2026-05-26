// <copyright file="GetCustomersQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Customers;

public sealed record GetCustomersQuery(
    string? Search = null,
    CustomerType? CustomerType = null,
    bool? IsVip = null,
    Guid? CompanyId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PagedResult<CustomerListItemDto>>;
