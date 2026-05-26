// <copyright file="CreateWorkTaskCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CreateWorkTask;

public sealed class CreateWorkTaskCommandValidator : AbstractValidator<CreateWorkTaskCommand>
{
    public CreateWorkTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
        RuleFor(x => x.DueAtUtc).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
    }
}
