// <copyright file="CustomerManagementOutboxMessageTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Domain.Outbox;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Outbox;

public sealed class CustomerManagementOutboxMessageTests
{
    [Fact]
    public void MarkProcessed_Should_Clear_Retry_And_Lease_State()
    {
        var message = CreateMessage();
        var now = DateTimeOffset.UtcNow;

        message.BeginProcessing(now.AddSeconds(30), "worker-1");
        message.MarkFailed("temporary", now.AddSeconds(5));
        message.BeginProcessing(now.AddSeconds(30), "worker-1");
        message.MarkProcessed(now);

        message.ProcessedAtUtc.Should().Be(now);
        message.LastError.Should().BeNull();
        message.NextAttemptAtUtc.Should().BeNull();
        message.LockedUntilUtc.Should().BeNull();
        message.LockedBy.Should().BeNull();
    }

    [Fact]
    public void MarkDeadLettered_Should_Stop_Retrying()
    {
        var message = CreateMessage();
        var now = DateTimeOffset.UtcNow;

        message.BeginProcessing(now.AddSeconds(30), "worker-1");
        message.MarkDeadLettered("poison", now);

        message.DeadLetteredAtUtc.Should().Be(now);
        message.NextAttemptAtUtc.Should().BeNull();
        message.LockedUntilUtc.Should().BeNull();
        message.AttemptCount.Should().Be(1);
    }

    private static CustomerManagementOutboxMessage CreateMessage()
        => CustomerManagementOutboxMessage.Create(
            Guid.NewGuid(),
            "notification.requested",
            1,
            "notification.requested.v1",
            "{}",
            DateTimeOffset.UtcNow,
            "trace",
            "idem");
}
