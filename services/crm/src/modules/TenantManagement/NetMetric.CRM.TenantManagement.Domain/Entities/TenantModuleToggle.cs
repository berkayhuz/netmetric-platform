// <copyright file="TenantModuleToggle.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.TenantManagement.Domain.Entities;

public sealed class TenantModuleToggle : EntityBase
{
    public string ModuleKey { get; private set; } = null!;
    public bool IsEnabled { get; private set; }

    private TenantModuleToggle() { }

    public TenantModuleToggle(Guid tenantId, string moduleKey, bool isEnabled)
    {
        TenantId = tenantId;
        ModuleKey = moduleKey.Trim();
        IsEnabled = isEnabled;
    }

    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;
}
