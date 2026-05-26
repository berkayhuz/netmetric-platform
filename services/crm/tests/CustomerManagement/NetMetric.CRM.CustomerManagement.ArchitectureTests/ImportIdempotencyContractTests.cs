// <copyright file="ImportIdempotencyContractTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCompanies;
using NetMetric.Idempotency;

namespace NetMetric.CRM.CustomerManagement.ArchitectureTests;

public sealed class ImportIdempotencyContractTests
{
    [Fact]
    public void All_Import_Commands_Should_Implement_IIdempotentCommand()
    {
        var importCommandTypes = typeof(ImportCompaniesCommand)
            .Assembly
            .GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsClass: true } &&
                type.Name.EndsWith("Command", StringComparison.Ordinal) &&
                type.Namespace is not null &&
                type.Namespace.Contains(".Features.Imports.Commands.", StringComparison.Ordinal))
            .ToList();

        importCommandTypes.Should().NotBeEmpty();
        importCommandTypes.Should().OnlyContain(type => typeof(IIdempotentCommand).IsAssignableFrom(type));
    }
}
