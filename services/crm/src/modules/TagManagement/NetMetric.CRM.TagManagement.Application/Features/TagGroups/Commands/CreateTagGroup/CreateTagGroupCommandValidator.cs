// <copyright file="CreateTagGroupCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.TagManagement.Application.Features.TagGroups.Commands.CreateTagGroup;

public sealed class CreateTagGroupCommandValidator : AbstractValidator<CreateTagGroupCommand>
{
    public CreateTagGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
