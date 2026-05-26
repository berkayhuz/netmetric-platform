// <copyright file="QuoteUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.Requests;


public sealed class QuoteUpsertRequest
{
    public string QuoteNumber { get; set; } = null!;
    public DateTime QuoteDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? TermsAndConditions { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1m;
    public List<QuoteItemRequest> Items { get; set; } = [];
    public string? RowVersion { get; set; }
}
