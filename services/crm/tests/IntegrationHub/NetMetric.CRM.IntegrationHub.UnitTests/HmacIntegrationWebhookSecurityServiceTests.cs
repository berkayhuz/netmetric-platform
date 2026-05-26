// <copyright file="HmacIntegrationWebhookSecurityServiceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.IntegrationHub.Infrastructure.Webhooks;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class HmacIntegrationWebhookSecurityServiceTests
{
    [Fact]
    public void CreateSignature_And_ValidateSignature_Should_Roundtrip()
    {
        var service = new HmacIntegrationWebhookSecurityService();
        var payload = "{\"event\":\"test\"}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = service.CreateSignature("secret-123", payload, timestamp);

        var valid = service.ValidateSignature("secret-123", payload, timestamp, $"sha256={signature}", DateTime.UtcNow, TimeSpan.FromMinutes(5));

        valid.Should().BeTrue();
    }
}
