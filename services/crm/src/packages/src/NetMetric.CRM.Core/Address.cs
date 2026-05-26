// <copyright file="Address.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Core;

public class Address : AuditableEntity
{
    public AddressType AddressType { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public bool IsDefault { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
}
