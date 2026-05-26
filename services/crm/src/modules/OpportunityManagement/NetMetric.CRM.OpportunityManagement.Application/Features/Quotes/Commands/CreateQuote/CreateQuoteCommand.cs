// <copyright file="CreateQuoteCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Commands.CreateQuote;

public sealed record CreateQuoteCommand(Guid OpportunityId, string QuoteNumber, DateTime QuoteDate, DateTime? ValidUntil, string? TermsAndConditions, Guid? OwnerUserId, string CurrencyCode, decimal ExchangeRate, IReadOnlyList<CreateQuoteItemModel> Items) : IRequest<QuoteDetailDto>;
