// <copyright file="ChangeOpportunityStageCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Application.Validators;

public sealed class ChangeOpportunityStageCommandValidator : AbstractValidator<ChangeOpportunityStageCommand>
{
    public ChangeOpportunityStageCommandValidator()
    {
        RuleFor(x => x.OpportunityId).NotEmpty();
        RuleFor(x => x.LostReasonId)
            .NotEmpty()
            .When(x => x.NewStage == OpportunityStageType.Lost)
            .WithMessage("Lost reason is required when moving an opportunity to Lost stage.");
    }
}
