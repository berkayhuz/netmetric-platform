// <copyright file="ApiKeyHashingServiceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.IntegrationHub.Application.Security;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class ApiKeyHashingServiceTests
{
    [Fact]
    public void CreateNew_Should_Return_Verifiable_NonEmpty_Key_Material()
    {
        var generated = ApiKeyHashingService.CreateNew();

        generated.PlaintextKey.Should().StartWith("nmk_");
        generated.Prefix.Should().NotBeNullOrWhiteSpace();
        generated.Salt.Should().NotBeNullOrWhiteSpace();
        generated.Hash.Should().NotBeNullOrWhiteSpace();
        ApiKeyHashingService.Verify(generated.PlaintextKey, generated.Salt, generated.Hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_Should_Fail_For_Tampered_Key()
    {
        var generated = ApiKeyHashingService.CreateNew();

        ApiKeyHashingService.Verify($"{generated.PlaintextKey}x", generated.Salt, generated.Hash).Should().BeFalse();
    }
}
