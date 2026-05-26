// <copyright file="ContractRecord.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.ContractLifecycle.Domain.Entities.Contracts;

public class ContractRecord : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? CustomerId { get; set; }
    public Guid? CompanyId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public DateTime? RenewalDateUtc { get; set; }
    public bool AutoRenew { get; set; }
    public decimal ContractValue { get; set; }
    public string Currency { get; set; } = "TRY";
    public Guid? OwnerUserId { get; set; }

    private ContractRecord() { }

    public ContractRecord(string code, string name, string? description = null)
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
