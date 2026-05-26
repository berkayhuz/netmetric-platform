// <copyright file="CreateCompanyCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Commands.Companies;
using NetMetric.CRM.CustomerManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Validators;

public sealed class CreateCompanyCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Fail_When_Name_Is_Empty()
    {
        var validator = new CreateCompanyCommandValidator();
        var command = new CreateCompanyCommand(string.Empty, null, null, null, null, null, null, null, null, null, null, CompanyType.Prospect, null, null);

        validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateCompanyCommandValidator();
        var command = new CreateCompanyCommand("Acme", null, null, "https://acme.test", "info@acme.test", "555", null, null, null, null, null, CompanyType.Customer, null, null);

        validator.Validate(command).IsValid.Should().BeTrue();
    }
}
