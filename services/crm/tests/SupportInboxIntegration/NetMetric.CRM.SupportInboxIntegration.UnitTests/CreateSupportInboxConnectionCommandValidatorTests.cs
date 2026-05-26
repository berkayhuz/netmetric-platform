// <copyright file="CreateSupportInboxConnectionCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using FluentAssertions;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.CreateSupportInboxConnection;
using NetMetric.CRM.SupportInboxIntegration.Application.Validators;
using NetMetric.CRM.SupportInboxIntegration.Domain.Enums;

namespace NetMetric.CRM.SupportInboxIntegration.UnitTests;

public sealed class CreateSupportInboxConnectionCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateSupportInboxConnectionCommandValidator(_ => [IPAddress.Parse("8.8.8.8")]);
        var result = validator.Validate(new CreateSupportInboxConnectionCommand("Support", SupportInboxProviderType.Imap, "support@example.com", "imap.example.com", 993, "user", "secret://crm/support-inbox/prod", true));
        result.IsValid.Should().BeTrue();
    }
}
