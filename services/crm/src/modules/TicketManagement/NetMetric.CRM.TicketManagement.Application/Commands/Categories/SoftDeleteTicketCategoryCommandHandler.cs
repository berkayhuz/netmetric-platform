// <copyright file="SoftDeleteTicketCategoryCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;


namespace NetMetric.CRM.TicketManagement.Application.Commands.Categories;

public sealed class SoftDeleteTicketCategoryCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<SoftDeleteTicketCategoryCommand>
{
    public async Task<Unit> Handle(SoftDeleteTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SoftDeleteCategoryAsync(request, cancellationToken);
        return Unit.Value;
    }

    Task IRequestHandler<SoftDeleteTicketCategoryCommand>.Handle(SoftDeleteTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        return Handle(request, cancellationToken);
    }
}
