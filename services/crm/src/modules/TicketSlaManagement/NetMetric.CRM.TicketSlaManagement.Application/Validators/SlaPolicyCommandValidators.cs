// <copyright file="SlaPolicyCommandValidators.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;

namespace NetMetric.CRM.TicketSlaManagement.Application.Validators;

public sealed class CreateSlaPolicyCommandValidator : AbstractValidator<CreateSlaPolicyCommand>
{
    public CreateSlaPolicyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
        RuleFor(x => x.FirstResponseTargetMinutes).GreaterThan(0);
        RuleFor(x => x.ResolutionTargetMinutes).GreaterThan(0);
        RuleFor(x => x.ResolutionTargetMinutes)
            .GreaterThanOrEqualTo(x => x.FirstResponseTargetMinutes);
    }
}

public sealed class UpdateSlaPolicyCommandValidator : AbstractValidator<UpdateSlaPolicyCommand>
{
    public UpdateSlaPolicyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
        RuleFor(x => x.FirstResponseTargetMinutes).GreaterThan(0);
        RuleFor(x => x.ResolutionTargetMinutes).GreaterThan(0);
        RuleFor(x => x.ResolutionTargetMinutes)
            .GreaterThanOrEqualTo(x => x.FirstResponseTargetMinutes);
    }
}
