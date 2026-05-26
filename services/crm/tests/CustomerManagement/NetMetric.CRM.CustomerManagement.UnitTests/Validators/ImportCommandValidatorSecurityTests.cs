// <copyright file="ImportCommandValidatorSecurityTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCompanies;
using NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportContacts;
using NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCustomers;

namespace NetMetric.CRM.CustomerManagement.Tests.Validators;

public sealed class ImportCommandValidatorSecurityTests
{
    [Theory]
    [InlineData(2_000_001, false)]
    [InlineData(32, true)]
    public void ImportCompanies_Should_Enforce_Max_Csv_Length(int csvLength, bool expectedValid)
    {
        var validator = new ImportCompaniesCommandValidator();
        var command = new ImportCompaniesCommand(Guid.NewGuid(), "idem", new string('a', csvLength));

        validator.Validate(command).IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData('|', false)]
    [InlineData(',', true)]
    [InlineData(';', true)]
    public void ImportContacts_Should_Only_Allow_Known_Separators(char separator, bool expectedValid)
    {
        var validator = new ImportContactsCommandValidator();
        var command = new ImportContactsCommand(Guid.NewGuid(), "idem", "FirstName,LastName\nAda,Lovelace", Separator: separator);

        validator.Validate(command).IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void ImportCustomers_Should_Reject_Excessive_Idempotency_Key_Length()
    {
        var validator = new ImportCustomersCommandValidator();
        var command = new ImportCustomersCommand(Guid.NewGuid(), new string('i', 201), "FirstName,LastName\nAda,Lovelace");

        validator.Validate(command).IsValid.Should().BeFalse();
    }
}
