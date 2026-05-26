// <copyright file="RenewalTracker.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.ContractLifecycle.Domain.Entities.Renewals;

public class RenewalTracker : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? CustomerId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime? RenewalDateUtc { get; set; }
    public string Status { get; set; } = "Open";
    public string RiskLevel { get; set; } = "Medium";
    public Guid? OwnerUserId { get; set; }

    private RenewalTracker() { }

    public RenewalTracker(string code, string name, string? description = null)
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
