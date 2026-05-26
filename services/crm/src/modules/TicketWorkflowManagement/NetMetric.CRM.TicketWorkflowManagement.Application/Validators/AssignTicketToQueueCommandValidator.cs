// <copyright file="AssignTicketToQueueCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Assignments.AssignTicketToQueue;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Validators;

public sealed class AssignTicketToQueueCommandValidator : AbstractValidator<AssignTicketToQueueCommand>
{
    public AssignTicketToQueueCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.NewQueueId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
