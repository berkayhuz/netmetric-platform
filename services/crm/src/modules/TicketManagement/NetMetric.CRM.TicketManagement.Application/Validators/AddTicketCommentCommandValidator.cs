// <copyright file="AddTicketCommentCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketManagement.Application.Features.Comments.Commands.AddTicketComment;

namespace NetMetric.CRM.TicketManagement.Application.Validators;

public sealed class AddTicketCommentCommandValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(4000);
    }
}
