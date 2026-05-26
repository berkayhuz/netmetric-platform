// <copyright file="AnalyticsProjectionBatch.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Projection;

namespace NetMetric.CRM.AnalyticsReporting.Application.Batchs;

public sealed record AnalyticsProjectionBatch(
    DateTime ProjectedAtUtc,
    IReadOnlyCollection<TenantProjection> Tenants,
    IReadOnlyCollection<SalesFunnelProjection> SalesFunnels,
    IReadOnlyCollection<CampaignRoiProjection> CampaignRoi,
    IReadOnlyCollection<RevenueAgingProjection> RevenueAging,
    IReadOnlyCollection<SupportKpiProjection> SupportKpis,
    IReadOnlyCollection<UserProductivityProjection> UserProductivity);
