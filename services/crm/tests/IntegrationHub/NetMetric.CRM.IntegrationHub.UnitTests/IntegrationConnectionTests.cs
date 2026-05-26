// <copyright file="IntegrationConnectionTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.IntegrationHub.Domain.Entities;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class IntegrationConnectionTests
{
    [Fact]
    public void Reconfigure_Should_Update_State()
    {
        var entity = new IntegrationConnection(Guid.NewGuid(), "erp.sap", "SAP", "erp", "{}");
        var settings = new { url = "x" };
        var json = System.Text.Json.JsonSerializer.Serialize(settings);

        entity.Reconfigure("SAP ERP", json, false);

        entity.DisplayName.Should().Be("SAP ERP");
        entity.SettingsJson.Should().Contain("\"url\":\"x\"");
        entity.IsEnabled.Should().BeFalse();
    }
}
