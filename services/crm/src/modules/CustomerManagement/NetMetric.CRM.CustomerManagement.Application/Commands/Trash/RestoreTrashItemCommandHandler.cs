// <copyright file="RestoreTrashItemCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Trash;

public sealed class RestoreTrashItemCommandHandler(IContactAdministrationService administrationService)
    : IRequestHandler<RestoreTrashItemCommand, Unit>
{
    public async Task<Unit> Handle(RestoreTrashItemCommand request, CancellationToken cancellationToken)
    {
        await administrationService.RestoreFromTrashAsync(request.TrashItemId, cancellationToken);
        return Unit.Value;
    }
}
