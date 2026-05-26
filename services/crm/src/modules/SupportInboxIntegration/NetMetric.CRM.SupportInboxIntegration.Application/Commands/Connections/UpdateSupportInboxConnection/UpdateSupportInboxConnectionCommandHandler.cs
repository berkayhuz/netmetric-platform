// <copyright file="UpdateSupportInboxConnectionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;
using NetMetric.Exceptions;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.UpdateSupportInboxConnection;

public sealed class UpdateSupportInboxConnectionCommandHandler(ISupportInboxIntegrationDbContext dbContext) : IRequestHandler<UpdateSupportInboxConnectionCommand, SupportInboxConnectionDto>
{
    public async Task<SupportInboxConnectionDto> Handle(UpdateSupportInboxConnectionCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Connections.FirstOrDefaultAsync(x => x.Id == request.ConnectionId, cancellationToken)
            ?? throw new NotFoundAppException("Support inbox connection not found.");
        entity.Update(request.Name, request.Host, request.Port, request.Username, request.SecretReference, request.UseSsl, request.IsActive);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new SupportInboxConnectionDto(entity.Id, entity.Name, entity.Provider.ToString(), entity.EmailAddress, entity.Host, entity.Port, entity.Username, entity.UseSsl, entity.IsActive);
    }
}
