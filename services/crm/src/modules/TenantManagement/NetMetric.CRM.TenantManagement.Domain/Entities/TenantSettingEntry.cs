// <copyright file="TenantSettingEntry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.TenantManagement.Domain.Entities;

public sealed class TenantSettingEntry : EntityBase
{
    public string Section { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    private TenantSettingEntry() { }

    public TenantSettingEntry(Guid tenantId, string section, string key, string value)
    {
        TenantId = tenantId;
        Section = section.Trim();
        Key = key.Trim();
        Value = value.Trim();
    }

    public void SetValue(string value) => Value = value.Trim();
}
