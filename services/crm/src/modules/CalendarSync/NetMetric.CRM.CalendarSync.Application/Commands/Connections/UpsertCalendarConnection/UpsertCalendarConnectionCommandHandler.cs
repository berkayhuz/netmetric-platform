// <copyright file="UpsertCalendarConnectionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CalendarSync.Application.Abstractions.Persistence;
using NetMetric.CRM.CalendarSync.Contracts.DTOs;
using NetMetric.CRM.CalendarSync.Domain.Entities;

namespace NetMetric.CRM.CalendarSync.Application.Commands.Connections.UpsertCalendarConnection;

public sealed class UpsertCalendarConnectionCommandHandler(ICalendarSyncDbContext dbContext) : IRequestHandler<UpsertCalendarConnectionCommand, CalendarConnectionDto>
{
    public async Task<CalendarConnectionDto> Handle(UpsertCalendarConnectionCommand request, CancellationToken cancellationToken)
    {
        CalendarConnection entity;

        if (request.Id.HasValue)
        {
            entity = await dbContext.Connections.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Calendar connection '{request.Id}' was not found.");

            entity.Update(request.Name, request.CalendarIdentifier, request.SecretReference, request.SyncDirection, request.IsActive);
        }
        else
        {
            entity = new CalendarConnection(request.Name, request.Provider, request.CalendarIdentifier, request.SecretReference, request.SyncDirection);
            entity.SetActive(request.IsActive);
            await dbContext.Connections.AddAsync(entity, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CalendarConnectionDto(entity.Id, entity.Name, entity.Provider.ToString(), entity.CalendarIdentifier, entity.SyncDirection.ToString(), entity.IsActive);
    }
}
