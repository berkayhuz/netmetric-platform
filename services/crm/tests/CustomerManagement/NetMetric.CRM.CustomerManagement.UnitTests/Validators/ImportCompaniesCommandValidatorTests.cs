// <copyright file="ImportCompaniesCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCompanies;

namespace NetMetric.CRM.CustomerManagement.Tests.Validators;

public sealed class ImportCompaniesCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Fail_When_Csv_Is_Empty()
    {
        var validator = new ImportCompaniesCommandValidator();
        var command = new ImportCompaniesCommand(Guid.NewGuid(), "idem-1", string.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var validator = new ImportCompaniesCommandValidator();
        var command = new ImportCompaniesCommand(Guid.NewGuid(), "idem-1", "Name\nAcme", Separator: ',');

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
