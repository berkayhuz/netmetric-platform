// <copyright file="MergeCommandsValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeCompanyRecords;
using NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeContactRecords;
using NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeCustomerRecords;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class MergeCompanyRecordsCommandValidator : AbstractValidator<MergeCompanyRecordsCommand>
{
    public MergeCompanyRecordsCommandValidator()
    {
        RuleFor(x => x.TargetCompanyId).NotEmpty();
        RuleFor(x => x.SourceCompanyId).NotEmpty().NotEqual(x => x.TargetCompanyId);
    }
}

public sealed class MergeContactRecordsCommandValidator : AbstractValidator<MergeContactRecordsCommand>
{
    public MergeContactRecordsCommandValidator()
    {
        RuleFor(x => x.TargetContactId).NotEmpty();
        RuleFor(x => x.SourceContactId).NotEmpty().NotEqual(x => x.TargetContactId);
    }
}

public sealed class MergeCustomerRecordsCommandValidator : AbstractValidator<MergeCustomerRecordsCommand>
{
    public MergeCustomerRecordsCommandValidator()
    {
        RuleFor(x => x.TargetCustomerId).NotEmpty();
        RuleFor(x => x.SourceCustomerId).NotEmpty().NotEqual(x => x.TargetCustomerId);
    }
}
