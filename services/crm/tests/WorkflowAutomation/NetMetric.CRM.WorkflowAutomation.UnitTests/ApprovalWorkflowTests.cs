// <copyright file="ApprovalWorkflowTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalWorkflows;

namespace NetMetric.CRM.WorkflowAutomation.UnitTests;

public sealed class ApprovalWorkflowTests
{
    [Fact]
    public void Create_Should_Set_Name()
    {
        var entity = ApprovalWorkflow.Create("Sample", "customer");

        entity.Name.Should().Be("Sample");
        entity.EntityType.Should().Be("customer");
        entity.Id.Should().NotBeEmpty();
    }
}
