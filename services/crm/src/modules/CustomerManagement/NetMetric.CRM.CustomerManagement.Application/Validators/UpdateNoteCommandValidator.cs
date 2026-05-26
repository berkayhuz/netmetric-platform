// <copyright file="UpdateNoteCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.UpdateNote;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.NoteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}
