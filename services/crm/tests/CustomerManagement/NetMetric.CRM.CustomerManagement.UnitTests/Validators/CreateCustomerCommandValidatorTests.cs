// <copyright file="CreateCustomerCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Commands.Customers;
using NetMetric.CRM.CustomerManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Validators;

public sealed class CreateCustomerCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Fail_When_LastName_Is_Missing()
    {
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand("Berkay", string.Empty, null, null, null, null, null, null, GenderType.Unknown, null, null, null, null, null, CustomerType.Individual, null, false, true, null);

        validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand("Berkay", "Huz", null, "berkay@test.com", "555", null, null, null, GenderType.Male, null, null, null, null, null, CustomerType.Individual, null, true, true, null);

        validator.Validate(command).IsValid.Should().BeTrue();
    }
}
