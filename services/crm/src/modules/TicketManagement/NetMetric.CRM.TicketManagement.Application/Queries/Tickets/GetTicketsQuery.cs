// <copyright file="GetTicketsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.TicketManagement.Application.Queries.Tickets;

public sealed record GetTicketsQuery(
    string? Search,
    TicketStatusType? Status,
    PriorityType? Priority,
    Guid? AssignedUserId,
    Guid? CustomerId,
    bool? IsActive,
    int Page,
    int PageSize) : IRequest<PagedResult<TicketListItemDto>>;
