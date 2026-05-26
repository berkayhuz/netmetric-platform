// <copyright file="LayerReferenceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.API.Controllers.CustomerManagement;
using NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCompanies;
using NetMetric.CRM.CustomerManagement.Infrastructure.DependencyInjection;

namespace NetMetric.CRM.CustomerManagement.ArchitectureTests;

public sealed class LayerReferenceTests
{
    [Fact]
    public void Application_Should_Not_Reference_Api_Or_Infrastructure()
    {
        var references = typeof(ImportCompaniesCommand)
            .Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        references.Should().NotContain(name => name.EndsWith(".CustomerManagement.API", StringComparison.OrdinalIgnoreCase));
        references.Should().NotContain(name => name.EndsWith(".CustomerManagement.Infrastructure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Infrastructure_Should_Not_Reference_Api()
    {
        var apiAssemblyName = typeof(CustomersController)
            .Assembly
            .GetName()
            .Name;

        var references = typeof(CustomerManagementModuleServiceCollectionExtensions)
            .Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        references.Should().NotContain(apiAssemblyName);
    }
}
