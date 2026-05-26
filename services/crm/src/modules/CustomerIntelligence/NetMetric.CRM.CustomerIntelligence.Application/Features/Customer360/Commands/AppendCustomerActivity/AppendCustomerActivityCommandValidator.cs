// <copyright file="AppendCustomerActivityCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Commands.AppendCustomerActivity;

public sealed class AppendCustomerActivityCommandValidator : AbstractValidator<AppendCustomerActivityCommand>
{
    public AppendCustomerActivityCommandValidator()
    {
        RuleFor(x => x.SubjectType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Channel).MaximumLength(64);
    }
}
