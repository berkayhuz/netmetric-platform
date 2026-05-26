// <copyright file="GetQuotesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;

public sealed record GetQuotesQuery(string? Search, QuoteStatusType? Status, Guid? OpportunityId, Guid? CustomerId, Guid? OwnerUserId, bool? IsActive, int Page, int PageSize) : IRequest<PagedResult<QuoteListItemDto>>;
