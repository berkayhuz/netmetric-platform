// <copyright file="TenantProfile.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.TenantManagement.Domain.Entities;

public sealed class TenantProfile : EntityBase
{
    public string Name { get; private set; } = null!;
    public string? PrimaryDomain { get; private set; }
    public string Locale { get; private set; } = "tr-TR";
    public string TimeZone { get; private set; } = "Europe/Istanbul";
    public string? BrandPrimaryColor { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsProvisioned { get; private set; }

    private TenantProfile() { }

    public TenantProfile(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name.Trim();
    }

    public void UpdateBranding(string? domain, string locale, string timeZone, string? brandPrimaryColor, string? logoUrl)
    {
        PrimaryDomain = string.IsNullOrWhiteSpace(domain) ? null : domain.Trim();
        Locale = locale.Trim();
        TimeZone = timeZone.Trim();
        BrandPrimaryColor = string.IsNullOrWhiteSpace(brandPrimaryColor) ? null : brandPrimaryColor.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
    }

    public void MarkProvisioned() => IsProvisioned = true;
}
