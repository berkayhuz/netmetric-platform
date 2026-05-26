// <copyright file="ProviderCredentialTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.IntegrationHub.Domain.Entities;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class ProviderCredentialTests
{
    [Fact]
    public void Constructor_Should_Create_Endpoint_Key_And_Enable_Credential()
    {
        var credential = new ProviderCredential(
            Guid.NewGuid(),
            "mock",
            "Mock Provider",
            "mock_access_token_sample_123",
            "mock_signing_secret_sample_123");

        credential.EndpointKey.Should().StartWith("ep_");
        credential.IsEnabled.Should().BeTrue();
        credential.ProviderKey.Should().Be("mock");
    }

    [Fact]
    public void Reconfigure_Should_Update_Values()
    {
        var credential = new ProviderCredential(
            Guid.NewGuid(),
            "mock",
            "Mock Provider",
            "mock_access_token_sample_123",
            "mock_signing_secret_sample_123");

        credential.Reconfigure("Updated Mock", "mock_access_token_sample_456", "mock_signing_secret_sample_456", false);

        credential.DisplayName.Should().Be("Updated Mock");
        credential.AccessToken.Should().Be("mock_access_token_sample_456");
        credential.WebhookSigningSecret.Should().Be("mock_signing_secret_sample_456");
        credential.IsEnabled.Should().BeFalse();
    }
}
