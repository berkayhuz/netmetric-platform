// <copyright file="GetQuotesByOpportunityQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Queries.GetQuotesByOpportunity;

public sealed record GetQuotesByOpportunityQuery(Guid OpportunityId) : IRequest<IReadOnlyList<QuoteDetailDto>>;
