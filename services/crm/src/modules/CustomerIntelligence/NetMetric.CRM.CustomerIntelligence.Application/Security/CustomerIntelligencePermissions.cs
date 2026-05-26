// <copyright file="CustomerIntelligencePermissions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Application.Security;

public static class CustomerIntelligencePermissions
{
    public const string DuplicatesRead = "customer-intelligence.duplicates.read";
    public const string DuplicatesManage = "customer-intelligence.duplicates.manage";
    public const string TimelineRead = "customer-intelligence.timeline.read";
    public const string SearchRead = "customer-intelligence.search.read";
    public const string ViewsManage = "customer-intelligence.views.manage";
    public const string HealthRead = "customer-intelligence.health.read";
    public const string RelationsManage = "customer-intelligence.relations.manage";
}
