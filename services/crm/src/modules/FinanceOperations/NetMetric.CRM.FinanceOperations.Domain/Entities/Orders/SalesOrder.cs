// <copyright file="SalesOrder.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.FinanceOperations.Domain.Entities.Orders;

public class SalesOrder : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? CustomerId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? RelatedDealId { get; set; }
    public Guid? RelatedQuoteId { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Open";

    private SalesOrder() { }

    public SalesOrder(string code, string name, string? description = null)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Update(string name, string? description)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
