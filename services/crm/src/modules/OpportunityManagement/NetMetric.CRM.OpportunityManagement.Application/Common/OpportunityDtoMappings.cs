// <copyright file="OpportunityDtoMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Application.Common;

public static class OpportunityDtoMappings
{
    public static OpportunityListItemDto ToListItemDto(this Opportunity opportunity, bool includeFinancialData = true)
        => new(opportunity.Id, opportunity.OpportunityCode, opportunity.Name, includeFinancialData ? opportunity.EstimatedAmount : null, includeFinancialData ? opportunity.ExpectedRevenue : null, opportunity.Probability, opportunity.Stage, opportunity.Status, opportunity.Priority, opportunity.EstimatedCloseDate, opportunity.LeadId, opportunity.CustomerId, opportunity.OwnerUserId, opportunity.IsActive);

    public static OpportunityProductDto ToDto(this OpportunityProduct item)
        => new(item.Id, item.ProductId, item.Quantity, item.UnitPrice, item.DiscountRate, item.VatRate);

    public static OpportunityContactDto ToDto(this OpportunityContact item)
        => new(item.Id, item.ContactId, item.IsDecisionMaker, item.IsPrimary);

    public static QuoteItemDto ToDto(this QuoteItem item)
        => new(item.Id, item.ProductId ?? Guid.Empty, item.Description, item.Quantity, item.UnitPrice, item.DiscountRate, item.TaxRate, item.LineTotal);

    public static QuoteDetailDto ToDto(this Quote quote, bool includeFinancialData = true, bool includeInternalNotes = true)
        => new(quote.Id, quote.QuoteNumber, quote.OpportunityId, quote.QuoteDate, quote.ValidUntil, includeFinancialData ? quote.SubTotal : null, includeFinancialData ? quote.DiscountTotal : null, includeFinancialData ? quote.TaxTotal : null, includeFinancialData ? quote.GrandTotal : null, includeInternalNotes ? quote.TermsAndConditions : null, quote.OwnerUserId, quote.CurrencyCode, includeFinancialData ? quote.ExchangeRate : null, quote.Items.Select(ToDto).ToList(), Convert.ToBase64String(quote.RowVersion));

    public static OpportunityStageHistoryDto ToDto(this OpportunityStageHistory history)
        => new(history.Id, history.OldStage, history.NewStage, history.ChangedAt, history.ChangedByUserId, history.Note);

    public static OpportunityDetailDto ToDetailDto(
        this Opportunity opportunity,
        IReadOnlyList<OpportunityProduct> products,
        IReadOnlyList<OpportunityContact> contacts,
        IReadOnlyList<Quote> quotes,
        IReadOnlyList<OpportunityStageHistory> stageHistory,
        bool includeFinancialData = true,
        bool includeInternalNotes = true)
        => new(
            opportunity.Id,
            opportunity.OpportunityCode,
            opportunity.Name,
            opportunity.Description,
            includeFinancialData ? opportunity.EstimatedAmount : null,
            includeFinancialData ? opportunity.ExpectedRevenue : null,
            opportunity.Probability,
            opportunity.EstimatedCloseDate,
            opportunity.Stage,
            opportunity.PipelineId,
            opportunity.PipelineStageId,
            opportunity.Status,
            opportunity.Priority,
            opportunity.LeadId,
            opportunity.CustomerId,
            opportunity.OwnerUserId,
            opportunity.LostReasonId,
            includeInternalNotes ? opportunity.LostNote : null,
            includeInternalNotes ? opportunity.Notes : null,
            opportunity.IsActive,
            products.Select(ToDto).ToList(),
            contacts.Select(ToDto).ToList(),
            quotes.Select(quote => quote.ToDto(includeFinancialData, includeInternalNotes)).ToList(),
            stageHistory.Select(ToDto).ToList(),
            Convert.ToBase64String(opportunity.RowVersion));

    public static LostReasonDto ToDto(this LostReason item) => new(item.Id, item.Name, item.Description, item.IsDefault);
}
