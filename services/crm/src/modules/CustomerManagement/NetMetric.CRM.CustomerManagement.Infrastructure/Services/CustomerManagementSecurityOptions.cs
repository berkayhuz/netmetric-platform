// <copyright file="CustomerManagementSecurityOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class CustomerManagementSecurityOptions
{
    public bool AllowUnassignedRead { get; set; } = true;
    public bool AllowUnassignedWrite { get; set; }
}
