// <copyright file="TicketSlaCommandValidators.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

namespace NetMetric.CRM.TicketSlaManagement.Application.Validators;

public sealed class AttachSlaToTicketCommandValidator : AbstractValidator<AttachSlaToTicketCommand>
{
    public AttachSlaToTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.SlaPolicyId).NotEmpty();
    }
}

public sealed class MarkFirstResponseCommandValidator : AbstractValidator<MarkFirstResponseCommand>
{
    public MarkFirstResponseCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
    }
}

public sealed class MarkResolvedCommandValidator : AbstractValidator<MarkResolvedCommand>
{
    public MarkResolvedCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
    }
}
