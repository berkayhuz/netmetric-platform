// <copyright file="GetLeadTimelineQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Features.Timeline.Queries.GetLeadTimeline;

public sealed class GetLeadTimelineQueryHandler(
    ILeadManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetLeadTimelineQuery, IReadOnlyList<LeadTimelineEventDto>>
{
    public async Task<IReadOnlyList<LeadTimelineEventDto>> Handle(GetLeadTimelineQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.LeadsResource);
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;

        var lead = await dbContext.Leads
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.LeadId, cancellationToken);

        if (lead is null)
            return Array.Empty<LeadTimelineEventDto>();

        var scoreEvents = await dbContext.LeadScores
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.LeadId == request.LeadId)
            .OrderByDescending(x => x.CalculatedAt)
            .Select(x => new LeadTimelineEventDto(
                x.CalculatedAt,
                "lead-score",
                "Lead score updated",
                !canSeeInternalNotes || x.ScoreReason == null
                    ? $"Score: {x.Score}"
                    : $"Score: {x.Score} - {x.ScoreReason}"))
            .ToListAsync(cancellationToken);

        var baseEvents = new List<LeadTimelineEventDto>
        {
            new(
                lead.CreatedAt,
                "lead-created",
                "Lead created",
                $"{lead.FullName} lead record created."),
            new(
                lead.UpdatedAt ?? lead.CreatedAt,
                "lead-status",
                "Lead status snapshot",
                $"Current status: {lead.Status}")
        };

        return baseEvents
            .Concat(scoreEvents)
            .OrderByDescending(x => x.OccurredAt)
            .ToList();
    }
}
