// <copyright file="QuoteUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.Requests;

public sealed class QuoteUpsertRequest
{
    public string QuoteNumber { get; set; } = null!;
    public string? ProposalTitle { get; set; }
    public string? ProposalSummary { get; set; }
    public string? ProposalBody { get; set; }
    public DateTime QuoteDate { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public string? TermsAndConditions { get; set; }
    public Guid? ProposalTemplateId { get; set; }
    public List<QuoteItemRequest> Items { get; set; } = new();
    public string? RowVersion { get; set; }
}
