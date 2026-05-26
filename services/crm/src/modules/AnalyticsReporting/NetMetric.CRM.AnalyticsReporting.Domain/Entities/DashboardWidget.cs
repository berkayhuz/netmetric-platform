// <copyright file="DashboardWidget.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.AnalyticsReporting.Domain.Entities;

public sealed class DashboardWidget : EntityBase
{
    public string WidgetKey { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string RoleName { get; private set; } = null!;
    public string DataSource { get; private set; } = null!;
    public string ConfigurationJson { get; private set; } = "{}";
    public int DisplayOrder { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private DashboardWidget() { }

    public DashboardWidget(string widgetKey, string title, string roleName, string dataSource, int displayOrder)
    {
        WidgetKey = widgetKey.Trim();
        Title = title.Trim();
        RoleName = roleName.Trim();
        DataSource = dataSource.Trim();
        DisplayOrder = displayOrder;
    }

    public void UpdateConfiguration(string title, string configurationJson, bool isEnabled, int displayOrder)
    {
        Title = title.Trim();
        ConfigurationJson = string.IsNullOrWhiteSpace(configurationJson) ? "{}" : configurationJson.Trim();
        IsEnabled = isEnabled;
        DisplayOrder = displayOrder;
    }
}
