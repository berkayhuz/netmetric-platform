// <copyright file="UpdateTicketCategoryCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Categories;

public sealed class UpdateTicketCategoryCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<UpdateTicketCategoryCommand, TicketCategoryDto>
{
    public Task<TicketCategoryDto> Handle(UpdateTicketCategoryCommand request, CancellationToken cancellationToken)
        => administrationService.UpdateCategoryAsync(request, cancellationToken);
}
