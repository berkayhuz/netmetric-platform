// <copyright file="TicketQueueTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Enums;

namespace NetMetric.CRM.TicketWorkflowManagement.UnitTests;

public sealed class TicketQueueTests
{
    [Fact]
    public void Update_Should_Change_Name_And_Default_Flag()
    {
        var queue = new TicketQueue("SUPPORT", "Support", TicketQueueAssignmentStrategy.Manual);

        queue.Update("Tier 1 Support", "Default queue", TicketQueueAssignmentStrategy.RoundRobin, true);

        queue.Name.Should().Be("Tier 1 Support");
        queue.Description.Should().Be("Default queue");
        queue.IsDefault.Should().BeTrue();
        queue.AssignmentStrategy.Should().Be(TicketQueueAssignmentStrategy.RoundRobin);
    }
}
