// <copyright file="BulkAssignTicketsOwnerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkAssignTicketsOwner;

public sealed class BulkAssignTicketsOwnerCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<BulkAssignTicketsOwnerCommand, int>
{
    public Task<int> Handle(BulkAssignTicketsOwnerCommand request, CancellationToken cancellationToken)
        => administrationService.BulkAssignOwnerAsync(request, cancellationToken);
}
