// <copyright file="GetCustomer360WorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Queries.GetCustomer360Workspace;

public sealed class GetCustomer360WorkspaceQueryHandler(ICustomerIntelligenceDbContext dbContext) : IRequestHandler<GetCustomer360WorkspaceQuery, Customer360WorkspaceDto>
{
    public async Task<Customer360WorkspaceDto> Handle(GetCustomer360WorkspaceQuery request, CancellationToken cancellationToken)
    {
        var activities = await dbContext.CustomerTimelineEntrys
            .AsNoTracking()
            .Where(x => x.SubjectType == "Customer" && x.SubjectId == request.CustomerId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        var relationships = await dbContext.CustomerRelationships
            .AsNoTracking()
            .Where(x => (x.SourceEntityType == "Customer" && x.SourceEntityId == request.CustomerId) || (x.TargetEntityType == "Customer" && x.TargetEntityId == request.CustomerId))
            .OrderByDescending(x => x.StrengthScore)
            .ThenByDescending(x => x.OccurredAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        var events = await dbContext.BehavioralEvents
            .AsNoTracking()
            .Where(x => x.SubjectType == "Customer" && x.SubjectId == request.CustomerId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        var identities = await dbContext.IdentityProfiles
            .AsNoTracking()
            .Where(x => x.SubjectType == "Customer" && x.SubjectId == request.CustomerId)
            .OrderByDescending(x => x.ConfidenceScore)
            .ThenByDescending(x => x.LastResolvedAtUtc)
            .ToListAsync(cancellationToken);

        return new Customer360WorkspaceDto(
            request.CustomerId,
            activities.Select(x => x.ToDto()).ToList(),
            relationships.Select(x => x.ToDto()).ToList(),
            events.Select(x => x.ToDto()).ToList(),
            identities.Select(x => x.ToDto()).ToList());
    }
}
