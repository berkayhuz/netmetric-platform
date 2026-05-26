// <copyright file="RegisterWebhookCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using FluentAssertions;
using NetMetric.CRM.IntegrationHub.Application.Commands.RegisterWebhook;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class RegisterWebhookCommandValidatorTests
{
    private readonly RegisterWebhookCommandValidator _validator = new(_ => [IPAddress.Parse("93.184.216.34")]);

    [Theory]
    [InlineData("https://hooks.example.com/webhook", true)]
    [InlineData("http://hooks.example.com/webhook", false)]
    [InlineData("https://localhost/webhook", false)]
    [InlineData("https://127.0.0.1/webhook", false)]
    [InlineData("https://hooks.internal/webhook", false)]
    [InlineData("https://user:pass@hooks.example.com/webhook", false)]
    [InlineData("https://hooks", false)]
    public void Validate_Should_Enforce_Safe_Target_Urls(string targetUrl, bool expectedValid)
    {
        var command = new RegisterWebhookCommand(Guid.NewGuid(), "Test", "deal.created", targetUrl, new string('s', 32), 10, 3);

        var result = _validator.Validate(command);

        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("127.0.0.1")]
    [InlineData("169.254.1.1")]
    [InlineData("172.16.0.1")]
    [InlineData("192.168.1.10")]
    [InlineData("100.64.0.1")]
    public void Validate_Should_Block_Dns_Rebinding_To_Non_Public_Addresses(string resolvedAddress)
    {
        var validator = new RegisterWebhookCommandValidator(_ => [IPAddress.Parse(resolvedAddress)]);
        var command = new RegisterWebhookCommand(Guid.NewGuid(), "Test", "deal.created", "https://hooks.example.com/webhook", new string('s', 32), 10, 3);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
