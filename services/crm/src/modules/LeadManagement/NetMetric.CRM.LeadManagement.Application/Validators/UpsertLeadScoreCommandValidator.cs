// <copyright file="UpsertLeadScoreCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;

namespace NetMetric.CRM.LeadManagement.Application.Validators;

public sealed class UpsertLeadScoreCommandValidator : AbstractValidator<UpsertLeadScoreCommand>
{
    public UpsertLeadScoreCommandValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(0, 100);
        RuleFor(x => x.ScoreReason).MaximumLength(500);
    }
}
