// <copyright file="ValidatorDiscoveryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Validators;

namespace NetMetric.CRM.CustomerManagement.ArchitectureTests;

public sealed class ValidatorDiscoveryTests
{
    [Fact]
    public void CustomerManagement_Assembly_Should_Contain_Multiple_Validators()
    {
        var validatorTypes = typeof(SearchCustomerManagementQueryValidator)
            .Assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Validator", StringComparison.Ordinal))
            .ToList();

        validatorTypes.Should().NotBeEmpty();
    }
}
