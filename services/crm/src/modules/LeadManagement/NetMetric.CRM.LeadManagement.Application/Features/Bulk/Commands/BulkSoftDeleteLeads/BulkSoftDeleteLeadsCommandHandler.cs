// <copyright file="BulkSoftDeleteLeadsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkSoftDeleteLeads;

public sealed class BulkSoftDeleteLeadsCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<BulkSoftDeleteLeadsCommand, int>
{
    public Task<int> Handle(BulkSoftDeleteLeadsCommand request, CancellationToken cancellationToken)
        => administrationService.BulkSoftDeleteAsync(request, cancellationToken);
}
