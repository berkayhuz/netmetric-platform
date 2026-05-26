// <copyright file="CompatibilitySurfaceRemovalTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Controllers.Deals;

namespace NetMetric.CRM.TicketWorkflowManagement.ArchitectureTests;

public sealed class CompatibilitySurfaceRemovalTests
{
    [Fact]
    public void TicketWorkflow_Compatibility_Controllers_Should_Not_Be_Public_Surface()
    {
        FindTicketControllers(type => type.Name.StartsWith("TicketWorkflow", StringComparison.Ordinal))
            .Should()
            .NotBeEmpty();
    }

    private static IReadOnlyList<Type> FindTicketControllers(Func<Type, bool> predicate) =>
        typeof(DealsController).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(ControllerBase).IsAssignableFrom(type)
                && type.Namespace == "NetMetric.CRM.API.Controllers.Tickets"
                && predicate(type))
            .ToList();
}
