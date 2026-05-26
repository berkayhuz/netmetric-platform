// <copyright file="Permissions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

internal static class Permissions
{
    public const string AuditLogsRead = "customer-management.audit-logs.read";
    public const string FieldSecurityBypass = "customer-management.field-security.bypass";
    public const string RowSecurityBypass = "customer-management.row-security.bypass";
    public const string SensitiveDataManage = "customer-management.sensitive-data.manage";
    public const string SensitiveDataView = "customer-management.sensitive-data.view";
}
