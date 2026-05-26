// <copyright file="CompanyUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.Requests;

public sealed class CompanyUpsertRequest
{
    public string Name { get; set; } = null!;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Sector { get; set; }
    public string? EmployeeCountRange { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public CompanyType CompanyType { get; set; } = CompanyType.Prospect;
    public Guid? OwnerUserId { get; set; }
    public Guid? ParentCompanyId { get; set; }
    public string? RowVersion { get; set; }
}
