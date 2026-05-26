// <copyright file="CreateNoteCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.CreateNote;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}
