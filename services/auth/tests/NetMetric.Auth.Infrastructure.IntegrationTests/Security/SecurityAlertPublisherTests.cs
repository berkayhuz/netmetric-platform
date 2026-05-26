// <copyright file="SecurityAlertPublisherTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Auth.Application.Options;
using NetMetric.Auth.Application.Records;
using NetMetric.Auth.Infrastructure.Services;
using NetMetric.Auth.TestKit.Logging;

namespace NetMetric.Auth.Infrastructure.IntegrationTests.Security;

public sealed class SecurityAlertPublisherTests
{
    [Fact]
    public async Task PublishAsync_When_MetadataContainsSensitiveValues_Should_Redact_LogPayload()
    {
        var sensitiveValue = BuildSampleValue("refresh", "redaction");
        var bearerValue = BuildSampleValue("bearer", "redaction");
        var headerValue = BuildSampleValue("__Secure-netmetric-refresh=", "redaction");

        var sink = new TestLogSink();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(sink));
        var publisher = new SecurityAlertPublisher(
            loggerFactory.CreateLogger<SecurityAlertPublisher>(),
            Options.Create(new SecurityAlertOptions { EnableStructuredAlerts = true }));

        await publisher.PublishAsync(
            new SecurityAlert(
                "auth.refresh-reuse",
                "critical",
                $"Refresh replay detected. authorization=Bearer {bearerValue}",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "correlation-id",
                "trace-id",
                $"refreshToken={sensitiveValue};cookie={headerValue};path=/api/auth/refresh"),
            CancellationToken.None);

        var logPayload = string.Join(Environment.NewLine, sink.Entries.Select(entry => entry.Message));

        logPayload.Should().Contain("refreshToken=[redacted]");
        logPayload.Should().Contain("cookie=[redacted]");
        logPayload.Should().Contain("authorization=[redacted]");
        logPayload.Should().NotContain(sensitiveValue);
        logPayload.Should().NotContain(bearerValue);
        logPayload.Should().NotContain(headerValue);
    }

    private static string BuildSampleValue(string prefix, string suffix) =>
        string.Join("-", prefix, "sample", "value", suffix);
}
