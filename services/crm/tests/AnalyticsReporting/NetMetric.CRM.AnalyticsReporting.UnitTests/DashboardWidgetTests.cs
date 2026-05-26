// <copyright file="DashboardWidgetTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.AnalyticsReporting.Domain.Entities;

namespace NetMetric.CRM.AnalyticsReporting.UnitTests;

public sealed class DashboardWidgetTests
{
    [Fact]
    public void UpdateConfiguration_Should_Update_State()
    {
        var widget = new DashboardWidget("sales-funnel", "Sales Funnel", "sales-manager", "sales-funnel", 1);
        widget.UpdateConfiguration("Pipeline", @"{""a"":1}", true, 2);

        widget.Title.Should().Be("Pipeline");
        widget.ConfigurationJson.Should().Contain(@"""a"":1");
        widget.DisplayOrder.Should().Be(2);
    }
}
