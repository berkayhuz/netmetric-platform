// <copyright file="CreateTicketQueueCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.CreateTicketQueue;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Validators;

public sealed class CreateTicketQueueCommandValidator : AbstractValidator<CreateTicketQueueCommand>
{
    public CreateTicketQueueCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
