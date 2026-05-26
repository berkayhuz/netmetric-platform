// <copyright file="QuoteDetailDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record QuoteDetailDto(Guid Id, string QuoteNumber, string? ProposalTitle, string? ProposalSummary, string? ProposalBody, QuoteStatusType Status, DateTime QuoteDate, DateTime? ValidUntil, decimal? SubTotal, decimal? DiscountTotal, decimal? TaxTotal, decimal? GrandTotal, string? TermsAndConditions, Guid? OpportunityId, Guid? CustomerId, Guid? OwnerUserId, string CurrencyCode, decimal? ExchangeRate, int RevisionNumber, Guid? ParentQuoteId, Guid? ProposalTemplateId, DateTime? SubmittedAt, DateTime? ApprovedAt, DateTime? SentAt, DateTime? AcceptedAt, DateTime? DeclinedAt, DateTime? ExpiredAt, string? RejectionReason, string? DeclineReason, IReadOnlyList<QuoteItemDto> Items, IReadOnlyList<QuoteStatusHistoryDto> History, string RowVersion);
