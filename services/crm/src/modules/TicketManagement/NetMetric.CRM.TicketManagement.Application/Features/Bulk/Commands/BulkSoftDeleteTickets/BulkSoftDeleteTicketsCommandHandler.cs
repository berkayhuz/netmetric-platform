// <copyright file="BulkSoftDeleteTicketsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkSoftDeleteTickets;

public sealed class BulkSoftDeleteTicketsCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<BulkSoftDeleteTicketsCommand, int>
{
    public Task<int> Handle(BulkSoftDeleteTicketsCommand request, CancellationToken cancellationToken)
        => administrationService.BulkSoftDeleteAsync(request, cancellationToken);
}
