// <copyright file="SalesQuota.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.SalesForecasting.Domain.Entities;

public class SalesQuota : AuditableEntity
{
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public Guid? OwnerUserId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? Notes { get; set; }

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
