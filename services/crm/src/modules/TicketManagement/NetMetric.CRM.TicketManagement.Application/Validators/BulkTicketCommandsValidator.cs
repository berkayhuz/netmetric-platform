// <copyright file="BulkTicketCommandsValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkAssignTicketsOwner;
using NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkSoftDeleteTickets;

namespace NetMetric.CRM.TicketManagement.Application.Validators;

public sealed class BulkAssignTicketsOwnerCommandValidator : AbstractValidator<BulkAssignTicketsOwnerCommand>
{
    public BulkAssignTicketsOwnerCommandValidator()
    {
        RuleFor(x => x.TicketIds).NotEmpty();
    }
}

public sealed class BulkSoftDeleteTicketsCommandValidator : AbstractValidator<BulkSoftDeleteTicketsCommand>
{
    public BulkSoftDeleteTicketsCommandValidator()
    {
        RuleFor(x => x.TicketIds).NotEmpty();
    }
}
