// <copyright file="DashboardSummaryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.AnalyticsReporting.Application.DTOs;

public sealed record DashboardSummaryDto(
    string RoleName,
    IReadOnlyCollection<WidgetDto> Widgets);

public sealed record WidgetDto(
    string WidgetKey,
    string Title,
    string DataSource,
    string ConfigurationJson,
    int DisplayOrder);
