// <copyright file="TicketSlaInstanceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;

namespace NetMetric.CRM.TicketSlaManagement.UnitTests.Domain;

public sealed class TicketSlaInstanceTests
{
    [Fact]
    public void Evaluate_Should_Mark_Breaches_When_Past_Due()
    {
        var now = DateTime.UtcNow;
        var entity = new TicketSlaInstance(Guid.NewGuid(), Guid.NewGuid(), now.AddMinutes(-10), now.AddMinutes(-5));

        entity.Evaluate(now);

        entity.IsFirstResponseBreached.Should().BeTrue();
        entity.IsResolutionBreached.Should().BeTrue();
    }
}
