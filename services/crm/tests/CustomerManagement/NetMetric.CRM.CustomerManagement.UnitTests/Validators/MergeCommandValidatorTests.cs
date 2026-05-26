// <copyright file="MergeCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeCompanyRecords;
using NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeContactRecords;
using NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeCustomerRecords;
using NetMetric.CRM.CustomerManagement.Application.Validators;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Validators;

public sealed class MergeCommandValidatorTests
{
    [Fact]
    public void MergeCompanyRecordsCommandValidator_Should_Fail_For_Same_Source_And_Target()
    {
        var id = Guid.NewGuid();
        var validator = new MergeCompanyRecordsCommandValidator();
        var command = new MergeCompanyRecordsCommand(id, id);

        validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void MergeContactRecordsCommandValidator_Should_Fail_For_Same_Source_And_Target()
    {
        var id = Guid.NewGuid();
        var validator = new MergeContactRecordsCommandValidator();
        var command = new MergeContactRecordsCommand(id, id);

        validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void MergeCustomerRecordsCommandValidator_Should_Fail_For_Same_Source_And_Target()
    {
        var id = Guid.NewGuid();
        var validator = new MergeCustomerRecordsCommandValidator();
        var command = new MergeCustomerRecordsCommand(id, id);

        validator.Validate(command).IsValid.Should().BeFalse();
    }
}
