// <copyright file="RecordTicketStatusChangeCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.StatusHistory.RecordTicketStatusChange;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Validators;

public sealed class RecordTicketStatusChangeCommandValidator : AbstractValidator<RecordTicketStatusChangeCommand>
{
    public RecordTicketStatusChangeCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.PreviousStatus).NotEmpty().MaximumLength(64);
        RuleFor(x => x.NewStatus).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
