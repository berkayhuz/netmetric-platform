// <copyright file="Quote.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class Quote : AuditableEntity
{
    public string QuoteNumber { get; set; } = string.Empty;
    public string? Title { get; set; }
    public QuoteStatusType Status { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? ProposalTemplateId { get; set; }
    public string? ProposalTitle { get; set; }
    public string? ProposalSummary { get; set; }
    public string? ProposalBody { get; set; }
    public string? TermsAndConditions { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1m;
    public decimal SubTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime QuoteDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeclinedAt { get; set; }
    public string? DeclineReason { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public int RevisionNumber { get; set; }
    public Guid? ParentQuoteId { get; set; }
    public Quote? ParentQuote { get; set; }
    public ICollection<QuoteItem> Items { get; set; } = [];
}
