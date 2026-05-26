// <copyright file="Company.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Core;

public class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
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
    public string? LogoStorageKey { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? LogoContentType { get; private set; }
    public CompanyType CompanyType { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? ParentCompanyId { get; set; }
    public Company? ParentCompany { get; set; }
    public ICollection<Company> ChildCompanies { get; set; } = [];
    public ICollection<Contact> Contacts { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    public void SetLogo(string? storageKey, string? logoUrl, string? contentType)
    {
        LogoStorageKey = string.IsNullOrWhiteSpace(storageKey) ? null : storageKey.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        LogoContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim();
    }
}
