// <copyright file="CreateContractRecordCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Commands.CreateContractRecord;

public sealed class CreateContractRecordCommandValidator : AbstractValidator<CreateContractRecordCommand>
{
    public CreateContractRecordCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
