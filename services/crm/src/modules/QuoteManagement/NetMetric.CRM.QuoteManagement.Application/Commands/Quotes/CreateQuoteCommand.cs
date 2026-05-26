// <copyright file="CreateQuoteCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

public sealed record CreateQuoteCommand(string QuoteNumber, string? ProposalTitle, string? ProposalSummary, string? ProposalBody, DateTime QuoteDate, DateTime? ValidUntil, Guid? OpportunityId, Guid? CustomerId, Guid? OwnerUserId, string CurrencyCode, decimal ExchangeRate, string? TermsAndConditions, Guid? ProposalTemplateId, IReadOnlyList<QuoteLineInput> Items) : IRequest<QuoteDetailDto>;
