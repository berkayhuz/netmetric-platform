// <copyright file="WebhookSubscriptionTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.IntegrationHub.Domain.Entities;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class WebhookSubscriptionTests
{
    [Fact]
    public void Constructor_Should_Preserve_Secret_For_Protected_Storage_And_Signing()
    {
        var subscription = new WebhookSubscription(
            Guid.NewGuid(),
            "Outbound Hook",
            "deal.created",
            "https://hooks.example.com/crm",
            "super-secret-key-123456",
            10,
            3);

        subscription.SecretKey.Should().Be("super-secret-key-123456");
        subscription.TimeoutSeconds.Should().Be(10);
        subscription.MaxAttempts.Should().Be(3);
    }

    [Fact]
    public void MarkDeliveryResult_Should_Set_Success_And_Failure_Timestamps()
    {
        var subscription = new WebhookSubscription(
            Guid.NewGuid(),
            "Outbound Hook",
            "deal.created",
            "https://hooks.example.com/crm",
            "super-secret-key-123456",
            10,
            3);

        var now = DateTime.UtcNow;
        subscription.MarkDeliveryResult(true, now);
        subscription.LastSuccessAtUtc.Should().Be(now);
        subscription.MarkDeliveryResult(false, now.AddMinutes(1));
        subscription.LastFailureAtUtc.Should().Be(now.AddMinutes(1));
    }
}
