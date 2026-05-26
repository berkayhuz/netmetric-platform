// <copyright file="DetectDuplicatesCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Duplicates.Commands.DetectDuplicates;

public sealed class DetectDuplicatesCommandValidator : AbstractValidator<DetectDuplicatesCommand>
{
    public DetectDuplicatesCommandValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
    }
}
