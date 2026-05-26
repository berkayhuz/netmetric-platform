// <copyright file="SearchLayeringTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetArchTest.Rules;
using NetMetric.Search.API.Endpoints;
using NetMetric.Search.Application.DependencyInjection;
using NetMetric.Search.Contracts;
using NetMetric.Search.Domain;
using NetMetric.Search.Infrastructure.DependencyInjection;
using NetMetric.Search.Worker;

namespace NetMetric.Search.ArchitectureTests.Layering;

public sealed class SearchLayeringTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Application_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(typeof(SearchDomainMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("NetMetric.Search.Application", "NetMetric.Search.Infrastructure", "NetMetric.Search.API", "NetMetric.Search.Worker")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(typeof(SearchApplicationServiceCollectionExtensions).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("NetMetric.Search.Infrastructure", "NetMetric.Search.API", "NetMetric.Search.Worker")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(typeof(SearchInfrastructureServiceCollectionExtensions).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("NetMetric.Search.API", "NetMetric.Search.Worker")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Domain_Application_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(typeof(SearchContractsMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("NetMetric.Search.Domain", "NetMetric.Search.Application", "NetMetric.Search.Infrastructure", "NetMetric.Search.API", "NetMetric.Search.Worker")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Worker_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(typeof(SearchWorkerMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("NetMetric.Search.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Worker()
    {
        var result = Types.InAssembly(typeof(SearchEndpoints).Assembly)
            .ShouldNot()
            .HaveDependencyOn("NetMetric.Search.Worker")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }
}
