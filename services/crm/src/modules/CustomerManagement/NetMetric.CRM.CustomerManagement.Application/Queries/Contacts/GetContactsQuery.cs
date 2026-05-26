// <copyright file="GetContactsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;

public sealed record GetContactsQuery(
    string? Search = null,
    Guid? CompanyId = null,
    Guid? CustomerId = null,
    bool? IsPrimary = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PagedResult<ContactListItemDto>>;
