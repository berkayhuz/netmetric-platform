// <copyright file="SearchCustomerManagementQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Search;
using NetMetric.Pagination;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Search.Queries.SearchCustomerManagement;

public sealed class SearchCustomerManagementQuery : IRequest<PagedResult<CustomerManagementSearchItemDto>>
{
    public string? Term { get; init; }
    public bool IncludeCompanies { get; init; } = true;
    public bool IncludeContacts { get; init; } = true;
    public bool IncludeCustomers { get; init; } = true;
    public bool? IsActive { get; init; }
    public Guid? OwnerUserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}
