// <copyright file="CreateSupportInboxConnectionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.CreateSupportInboxConnection;

public sealed class CreateSupportInboxConnectionCommandHandler(ISupportInboxIntegrationDbContext dbContext) : IRequestHandler<CreateSupportInboxConnectionCommand, SupportInboxConnectionDto>
{
    public async Task<SupportInboxConnectionDto> Handle(CreateSupportInboxConnectionCommand request, CancellationToken cancellationToken)
    {
        var entity = new SupportInboxConnection(request.Name, request.Provider, request.EmailAddress, request.Host, request.Port, request.Username, request.SecretReference, request.UseSsl);
        await dbContext.Connections.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new SupportInboxConnectionDto(entity.Id, entity.Name, entity.Provider.ToString(), entity.EmailAddress, entity.Host, entity.Port, entity.Username, entity.UseSsl, entity.IsActive);
    }
}
