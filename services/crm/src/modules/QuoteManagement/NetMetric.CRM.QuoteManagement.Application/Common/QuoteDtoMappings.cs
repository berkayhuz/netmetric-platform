// <copyright file="QuoteDtoMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.QuoteManagement.Domain.Entities;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.QuoteManagement.Application.Common;

public static class QuoteDtoMappings
{
    public static QuoteItemDto ToDto(this QuoteItem item)
        => new(item.Id, item.ProductId ?? Guid.Empty, item.Description, item.Quantity, item.UnitPrice, item.DiscountRate, item.TaxRate, item.LineTotal);

    public static QuoteStatusHistoryDto ToDto(this QuoteStatusHistory item)
        => new(item.Id, item.OldStatus, item.NewStatus, item.ChangedAt, item.ChangedByUserId, item.Note);

    public static QuoteListItemDto ToListItemDto(this Quote quote, bool includeFinancialData = true)
        => new(quote.Id, quote.QuoteNumber, quote.ProposalTitle, quote.Status, quote.QuoteDate, quote.ValidUntil, includeFinancialData ? quote.GrandTotal : null, quote.CurrencyCode, quote.OpportunityId, quote.CustomerId, quote.OwnerUserId, quote.RevisionNumber, quote.IsActive);

    public static ProposalTemplateDto ToDto(this ProposalTemplate template)
        => new(template.Id, template.Name, template.SubjectTemplate, template.BodyTemplate, template.IsDefault, template.IsActive, template.Notes);

    public static QuoteDetailDto ToDetailDto(
        this Quote quote,
        IReadOnlyList<QuoteStatusHistory> history,
        bool includeFinancialData = true,
        bool includeInternalNotes = true)
        => new(
            quote.Id,
            quote.QuoteNumber,
            quote.ProposalTitle,
            quote.ProposalSummary,
            quote.ProposalBody,
            quote.Status,
            quote.QuoteDate,
            quote.ValidUntil,
            includeFinancialData ? quote.SubTotal : null,
            includeFinancialData ? quote.DiscountTotal : null,
            includeFinancialData ? quote.TaxTotal : null,
            includeFinancialData ? quote.GrandTotal : null,
            includeInternalNotes ? quote.TermsAndConditions : null,
            quote.OpportunityId,
            quote.CustomerId,
            quote.OwnerUserId,
            quote.CurrencyCode,
            includeFinancialData ? quote.ExchangeRate : null,
            quote.RevisionNumber,
            quote.ParentQuoteId,
            quote.ProposalTemplateId,
            quote.SubmittedAt,
            quote.ApprovedAt,
            quote.SentAt,
            quote.AcceptedAt,
            quote.DeclinedAt,
            quote.ExpiredAt,
            includeInternalNotes ? quote.RejectionReason : null,
            includeInternalNotes ? quote.DeclineReason : null,
            quote.Items.Select(ToDto).ToList(),
            history.Select(x => includeInternalNotes ? x.ToDto() : x.ToDto() with { Note = null }).ToList(),
            Convert.ToBase64String(quote.RowVersion));

    public static QuoteWorkspaceDto ToWorkspaceDto(this Quote quote, IReadOnlyList<QuoteStatusHistory> history, bool includeFinancialData = true, bool includeInternalNotes = true)
    {
        var detail = quote.ToDetailDto(history, includeFinancialData, includeInternalNotes);
        return new QuoteWorkspaceDto(detail, QuoteStateMachine.CanEdit(quote.Status), QuoteStateMachine.CanSubmit(quote.Status), QuoteStateMachine.CanApprove(quote.Status), QuoteStateMachine.CanReject(quote.Status), QuoteStateMachine.CanSend(quote.Status), QuoteStateMachine.CanAccept(quote.Status), QuoteStateMachine.CanDecline(quote.Status), QuoteStateMachine.CanExpire(quote.Status));
    }
}
