// <copyright file="UpsertAiProviderCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ArtificialIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.ArtificialIntelligence.Contracts.DTOs;
using NetMetric.CRM.ArtificialIntelligence.Domain.Entities;

namespace NetMetric.CRM.ArtificialIntelligence.Application.Commands.Providers.UpsertAiProvider;

public sealed class UpsertAiProviderCommandHandler(IArtificialIntelligenceDbContext dbContext) : IRequestHandler<UpsertAiProviderCommand, AiProviderConnectionDto>
{
    public async Task<AiProviderConnectionDto> Handle(UpsertAiProviderCommand request, CancellationToken cancellationToken)
    {
        AiProviderConnection entity;

        if (request.Id.HasValue)
        {
            entity = await dbContext.ProviderConnections.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException($"AI provider '{request.Id}' was not found.");

            entity.Update(request.Name, request.ModelName, request.Endpoint, request.SecretReference, request.Capabilities, request.IsActive);
        }
        else
        {
            entity = new AiProviderConnection(request.Name, request.Provider, request.ModelName, request.Endpoint, request.SecretReference, request.Capabilities);
            entity.SetActive(request.IsActive);
            await dbContext.ProviderConnections.AddAsync(entity, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AiProviderConnectionDto(
            entity.Id,
            entity.Name,
            entity.Provider.ToString(),
            entity.ModelName,
            entity.Endpoint,
            entity.Capabilities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            entity.IsActive);
    }
}
