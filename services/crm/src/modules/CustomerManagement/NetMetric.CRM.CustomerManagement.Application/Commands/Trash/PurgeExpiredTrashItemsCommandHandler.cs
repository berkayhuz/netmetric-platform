// <copyright file="PurgeExpiredTrashItemsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Trash;

public sealed class PurgeExpiredTrashItemsCommandHandler(IContactAdministrationService administrationService)
    : IRequestHandler<PurgeExpiredTrashItemsCommand, int>
{
    public Task<int> Handle(PurgeExpiredTrashItemsCommand request, CancellationToken cancellationToken) =>
        request.TenantId.HasValue
            ? administrationService.PurgeExpiredTrashItemsForTenantAsync(request.TenantId.Value, request.BatchSize, cancellationToken)
            : administrationService.PurgeExpiredTrashItemsAsync(request.BatchSize, cancellationToken);
}
