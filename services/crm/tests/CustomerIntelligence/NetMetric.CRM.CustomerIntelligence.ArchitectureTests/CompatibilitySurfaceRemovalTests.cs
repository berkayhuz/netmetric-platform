// <copyright file="CompatibilitySurfaceRemovalTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Controllers.Deals;

namespace NetMetric.CRM.CustomerIntelligence.ArchitectureTests;

public sealed class CompatibilitySurfaceRemovalTests
{
    [Fact]
    public void CustomerIntelligence_Compatibility_Controllers_Should_Not_Be_Public_Surface()
    {
        FindControllers("NetMetric.CRM.API.Controllers.Customers").Should().NotBeEmpty();
    }

    private static IReadOnlyList<Type> FindControllers(string @namespace) =>
        typeof(DealsController).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(ControllerBase).IsAssignableFrom(type)
                && type.Namespace == @namespace)
            .ToList();
}
