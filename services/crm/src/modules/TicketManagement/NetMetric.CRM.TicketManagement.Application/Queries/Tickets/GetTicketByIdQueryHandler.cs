// <copyright file="GetTicketByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Application.Common;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Queries.Tickets;

public sealed class GetTicketByIdQueryHandler(
    ITicketManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetTicketByIdQuery, TicketDetailDto?>
{
    public async Task<TicketDetailDto?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.TicketsResource);
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;

        var ticket = await dbContext.Tickets
            .AsNoTracking()
            .Include(x => x.Comments)
            .ApplyRowScope(scope, x => x.TenantId, x => x.AssignedUserId, x => x.AssignedUserId)
            .FirstOrDefaultAsync(x => x.Id == request.TicketId, cancellationToken);

        return ticket?.ToDetailDto(canSeeInternalNotes);
    }
}
