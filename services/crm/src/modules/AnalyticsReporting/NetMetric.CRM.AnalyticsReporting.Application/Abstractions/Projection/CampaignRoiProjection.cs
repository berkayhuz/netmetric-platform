// <copyright file="CampaignRoiProjection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Projection;

public sealed record CampaignRoiProjection(
    Guid TenantId,
    string CampaignName,
    decimal Spend,
    decimal Revenue);
