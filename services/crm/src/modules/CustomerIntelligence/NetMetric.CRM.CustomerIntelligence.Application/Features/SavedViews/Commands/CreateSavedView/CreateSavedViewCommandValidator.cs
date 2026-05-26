// <copyright file="CreateSavedViewCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.SavedViews.Commands.CreateSavedView;

public sealed class CreateSavedViewCommandValidator : AbstractValidator<CreateSavedViewCommand>
{
    public CreateSavedViewCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FilterJson).NotEmpty().MaximumLength(200);
    }
}
