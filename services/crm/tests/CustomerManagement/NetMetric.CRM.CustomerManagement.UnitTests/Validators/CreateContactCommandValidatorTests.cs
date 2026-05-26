// <copyright file="CreateContactCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;
using NetMetric.CRM.CustomerManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Validators;

public sealed class CreateContactCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Fail_When_FirstName_Is_Missing()
    {
        var validator = new CreateContactCommandValidator();
        var command = new CreateContactCommand(string.Empty, "Huz", null, null, null, null, null, null, GenderType.Unknown, null, null, null, null, null, null, null, false);

        validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateContactCommandValidator();
        var command = new CreateContactCommand("Berkay", "Huz", null, "berkay@test.com", "555", null, null, null, GenderType.Male, null, null, null, null, null, null, Guid.NewGuid(), true);

        validator.Validate(command).IsValid.Should().BeTrue();
    }
}
