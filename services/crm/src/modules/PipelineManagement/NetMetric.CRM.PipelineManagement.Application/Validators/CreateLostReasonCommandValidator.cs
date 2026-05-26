// <copyright file="CreateLostReasonCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.PipelineManagement.Application.Commands;

namespace NetMetric.CRM.PipelineManagement.Application.Validators;

public sealed class CreateLostReasonCommandValidator : AbstractValidator<CreateLostReasonCommand>
{
    public CreateLostReasonCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(1024);
    }
}
