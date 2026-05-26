// <copyright file="SupportInboxConnectionTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;
using NetMetric.CRM.SupportInboxIntegration.Domain.Enums;

namespace NetMetric.CRM.SupportInboxIntegration.UnitTests;

public sealed class SupportInboxConnectionTests
{
    [Fact]
    public void Update_Should_Apply_New_Settings()
    {
        var connection = new SupportInboxConnection("Support", SupportInboxProviderType.Imap, "support@example.com", "imap.example.com", 993, "user", "secret", true);
        connection.Update("Support Main", "imap.internal", 993, "new-user", "new-secret", true, false);
        connection.Name.Should().Be("Support Main");
        connection.Host.Should().Be("imap.internal");
        connection.IsActive.Should().BeFalse();
    }
}
