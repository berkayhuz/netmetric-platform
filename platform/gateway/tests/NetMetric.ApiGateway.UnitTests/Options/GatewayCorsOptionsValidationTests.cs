// <copyright file="GatewayCorsOptionsValidationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NetMetric.ApiGateway.Options;

namespace NetMetric.ApiGateway.UnitTests.Options;

public sealed class GatewayCorsOptionsValidationTests
{
    [Fact]
    public void Validate_Should_Accept_Production_NetMetric_Origins()
    {
        var validator = new GatewayCorsOptionsValidation(new TestHostEnvironment(Environments.Production));

        var result = validator.Validate(
            null,
            new GatewayCorsOptions
            {
                AllowedOrigins =
                [
                    "https://netmetric.net",
                    "https://www.netmetric.net",
                    "https://auth.netmetric.net",
                    "https://account.netmetric.net",
                    "https://crm.netmetric.net",
                    "https://tools.netmetric.net",
                    "https://api.netmetric.net"
                ],
                AllowCredentials = true
            });

        result.Failed.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Reject_Development_Origin_In_Production()
    {
        var validator = new GatewayCorsOptionsValidation(new TestHostEnvironment(Environments.Production));

        var result = validator.Validate(
            null,
            new GatewayCorsOptions
            {
                AllowedOrigins = ["http://localhost:7006"],
                AllowCredentials = true
            });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(message => message.Contains("loopback origin", StringComparison.Ordinal));
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "NetMetric.ApiGateway.UnitTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
