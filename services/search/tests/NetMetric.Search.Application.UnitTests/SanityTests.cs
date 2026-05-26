// <copyright file="SanityTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Application.DependencyInjection;

namespace NetMetric.Search.Application.UnitTests;

public sealed class SanityTests
{
    [Fact]
    public void AddSearchApplication_Should_Return_ServiceCollection()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        var result = services.AddSearchApplication();

        Assert.Same(services, result);
    }
}
