// <copyright file="TrackBehavioralEventCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.BehavioralEvents;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Cdp.Commands.TrackBehavioralEvent;

public sealed class TrackBehavioralEventCommandHandler(ICustomerIntelligenceDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<TrackBehavioralEventCommand, BehavioralEventDto>
{
    public async Task<BehavioralEventDto> Handle(TrackBehavioralEventCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = new BehavioralEvent
        {
            TenantId = currentUserService.TenantId,
            Source = request.Source.Trim(),
            EventName = request.EventName.Trim(),
            SubjectType = request.SubjectType.Trim(),
            SubjectId = request.SubjectId,
            IdentityKey = string.IsNullOrWhiteSpace(request.IdentityKey) ? null : request.IdentityKey.Trim(),
            Channel = string.IsNullOrWhiteSpace(request.Channel) ? null : request.Channel.Trim(),
            PropertiesJson = string.IsNullOrWhiteSpace(request.PropertiesJson) ? null : request.PropertiesJson.Trim(),
            OccurredAtUtc = request.OccurredAtUtc?.ToUniversalTime() ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };

        await dbContext.BehavioralEvents.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
