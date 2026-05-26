// <copyright file="UpdateTicketCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

namespace NetMetric.CRM.TicketManagement.Application.Validators;

public sealed class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ResolveDueAt)
            .GreaterThanOrEqualTo(x => x.FirstResponseDueAt!.Value)
            .When(x => x.ResolveDueAt.HasValue && x.FirstResponseDueAt.HasValue);
    }
}
