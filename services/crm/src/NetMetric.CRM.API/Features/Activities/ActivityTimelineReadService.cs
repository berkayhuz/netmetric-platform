// <copyright file="ActivityTimelineReadService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.API.Contracts.Activities;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Timeline;
using NetMetric.CRM.CustomerManagement.Application.Features.Timeline.Queries.GetEntityTimeline;
using NetMetric.CRM.DealManagement.Application.Queries.Deals;
using NetMetric.CRM.LeadManagement.Application.Features.Timeline.Queries.GetLeadTimeline;
using NetMetric.CRM.OpportunityManagement.Application.Features.Timeline.Queries.GetOpportunityTimeline;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.TicketManagement.Application.Features.Timeline.Queries.GetTicketTimeline;
using NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTaskById;
using NetMetric.CRM.WorkManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.API.Features.Activities;

public interface IActivityTimelineReadService
{
    Task<ActivityTimelineFeedDto> GetGlobalAsync(
        string? type,
        string? sourceModule,
        Guid? ownerUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ActivityTimelineItemDto?> GetByIdAsync(Guid activityId, CancellationToken cancellationToken);

    Task<ActivityTimelineFeedDto> GetRelatedAsync(
        ActivityEntityType entityType,
        Guid entityId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

public sealed class ActivityTimelineReadService(
    IMediator mediator,
    WorkManagementDbContext workManagementDbContext) : IActivityTimelineReadService
{
    public async Task<ActivityTimelineFeedDto> GetGlobalAsync(
        string? type,
        string? sourceModule,
        Guid? ownerUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 200);
        var normalizedSourceModule = sourceModule?.Trim();
        var normalizedType = type?.Trim();

        var query = workManagementDbContext.Activities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedType))
        {
            query = query.Where(x => x.ActivityType.ToString() == normalizedType);
        }

        if (!string.IsNullOrWhiteSpace(normalizedSourceModule))
        {
            if (!string.Equals(normalizedSourceModule, "work-management", StringComparison.OrdinalIgnoreCase))
            {
                return new ActivityTimelineFeedDto([], 0, normalizedPage, normalizedPageSize);
            }
        }

        if (ownerUserId.HasValue)
        {
            var ownerUserIdText = ownerUserId.Value.ToString();
            query = query.Where(x => x.CreatedBy == ownerUserIdText);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc <= toUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => ToCanonicalFromActivityLog(x.Id, x.OccurredAtUtc, x.ActivityType.ToString(), x.Subject, x.RelatedEntityId, x.CreatedBy))
            .ToListAsync(cancellationToken);

        return new ActivityTimelineFeedDto(items, totalCount, normalizedPage, normalizedPageSize);
    }

    public async Task<ActivityTimelineItemDto?> GetByIdAsync(Guid activityId, CancellationToken cancellationToken)
    {
        var entity = await workManagementDbContext.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == activityId, cancellationToken);

        return entity is null
            ? null
            : ToCanonicalFromActivityLog(entity.Id, entity.OccurredAtUtc, entity.ActivityType.ToString(), entity.Subject, entity.RelatedEntityId, entity.CreatedBy);
    }

    public async Task<ActivityTimelineFeedDto> GetRelatedAsync(
        ActivityEntityType entityType,
        Guid entityId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 200);
        IReadOnlyList<ActivityTimelineItemDto> entries = entityType switch
        {
            ActivityEntityType.Lead => await GetLeadEntriesAsync(entityId, cancellationToken),
            ActivityEntityType.Deal => await GetDealEntriesAsync(entityId, cancellationToken),
            ActivityEntityType.Opportunity => await GetOpportunityEntriesAsync(entityId, cancellationToken),
            ActivityEntityType.Quote => await GetQuoteEntriesAsync(entityId, cancellationToken),
            ActivityEntityType.Ticket => await GetTicketEntriesAsync(entityId, cancellationToken),
            ActivityEntityType.Customer => await GetCustomerManagementEntityEntriesAsync("customer", entityId, cancellationToken),
            ActivityEntityType.Company => await GetCustomerManagementEntityEntriesAsync("company", entityId, cancellationToken),
            ActivityEntityType.Contact => await GetCustomerManagementEntityEntriesAsync("contact", entityId, cancellationToken),
            ActivityEntityType.Task => await GetTaskEntriesAsync(entityId, cancellationToken),
            _ => []
        };
        var activityLogEntries = await workManagementDbContext.Activities
            .AsNoTracking()
            .Where(x => x.RelatedEntityId == entityId)
            .Select(x => ToCanonicalFromActivityLogForRelated(
                x.Id,
                x.OccurredAtUtc,
                x.ActivityType.ToString(),
                x.Subject,
                entityType.ToString().ToLowerInvariant(),
                entityId,
                x.CreatedBy))
            .ToListAsync(cancellationToken);

        entries = entries.Concat(activityLogEntries).ToList();

        var filtered = entries.AsEnumerable();
        if (fromUtc.HasValue)
        {
            filtered = filtered.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            filtered = filtered.Where(x => x.OccurredAtUtc <= toUtc.Value);
        }

        var ordered = filtered
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .ToList();

        var totalCount = ordered.Count;
        var paged = ordered
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return new ActivityTimelineFeedDto(paged, totalCount, normalizedPage, normalizedPageSize);
    }

    private async Task<IReadOnlyList<ActivityTimelineItemDto>> GetLeadEntriesAsync(Guid leadId, CancellationToken cancellationToken)
    {
        var timeline = await mediator.Send(new GetLeadTimelineQuery(leadId), cancellationToken);
        return timeline
            .Select((x, index) => new ActivityTimelineItemDto(
                Id: $"lead:{leadId}:{index}",
                OccurredAtUtc: x.OccurredAt,
                Type: x.EventType,
                Title: x.Title,
                Description: x.Description,
                Status: null,
                SourceModule: "lead-management",
                SourceEntityType: "lead",
                SourceEntityId: leadId,
                ActorUserId: null,
                OwnerUserId: null,
                RelatedRecords: [new ActivityRelatedRecordDto("lead", leadId, null, "subject")],
                Metadata: new Dictionary<string, string?>()))
            .ToList();
    }

    private async Task<IReadOnlyList<ActivityTimelineItemDto>> GetDealEntriesAsync(Guid dealId, CancellationToken cancellationToken)
    {
        var timeline = await mediator.Send(new GetDealTimelineQuery(dealId), cancellationToken);
        return timeline
            .Select((x, index) => new ActivityTimelineItemDto(
                Id: $"deal:{x.Id}:{index}",
                OccurredAtUtc: x.OccurredAt,
                Type: "deal-outcome",
                Title: $"Deal {x.Outcome}",
                Description: x.Note,
                Status: x.Stage,
                SourceModule: "deal-management",
                SourceEntityType: "deal",
                SourceEntityId: dealId,
                ActorUserId: x.ChangedByUserId,
                OwnerUserId: null,
                RelatedRecords: [new ActivityRelatedRecordDto("deal", dealId, null, "subject")],
                Metadata: new Dictionary<string, string?> { ["outcome"] = x.Outcome, ["stage"] = x.Stage }))
            .ToList();
    }

    private async Task<IReadOnlyList<ActivityTimelineItemDto>> GetOpportunityEntriesAsync(Guid opportunityId, CancellationToken cancellationToken)
    {
        var timeline = await mediator.Send(new GetOpportunityTimelineQuery(opportunityId), cancellationToken);
        return timeline
            .Select((x, index) => new ActivityTimelineItemDto(
                Id: $"opportunity:{opportunityId}:{index}",
                OccurredAtUtc: x.OccurredAt,
                Type: x.EventType,
                Title: x.Title,
                Description: x.Description,
                Status: null,
                SourceModule: "opportunity-management",
                SourceEntityType: "opportunity",
                SourceEntityId: opportunityId,
                ActorUserId: null,
                OwnerUserId: null,
                RelatedRecords: [new ActivityRelatedRecordDto("opportunity", opportunityId, null, "subject")],
                Metadata: new Dictionary<string, string?>()))
            .ToList();
    }

    private async Task<IReadOnlyList<ActivityTimelineItemDto>> GetQuoteEntriesAsync(Guid quoteId, CancellationToken cancellationToken)
    {
        var timeline = await mediator.Send(new GetQuoteTimelineQuery(quoteId), cancellationToken);
        return timeline
            .Select((x, index) => new ActivityTimelineItemDto(
                Id: $"quote:{quoteId}:{index}",
                OccurredAtUtc: x.OccurredAt,
                Type: x.EventType,
                Title: x.Title,
                Description: x.Description,
                Status: x.EventType,
                SourceModule: "quote-management",
                SourceEntityType: "quote",
                SourceEntityId: quoteId,
                ActorUserId: null,
                OwnerUserId: null,
                RelatedRecords: [new ActivityRelatedRecordDto("quote", quoteId, null, "subject")],
                Metadata: new Dictionary<string, string?>()))
            .ToList();
    }

    private async Task<IReadOnlyList<ActivityTimelineItemDto>> GetTicketEntriesAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var timeline = await mediator.Send(new GetTicketTimelineQuery(ticketId), cancellationToken);
        return timeline
            .Select((x, index) => new ActivityTimelineItemDto(
                Id: $"ticket:{ticketId}:{index}",
                OccurredAtUtc: x.OccurredAt,
                Type: x.EventType,
                Title: x.Title,
                Description: x.Description,
                Status: null,
                SourceModule: "ticket-management",
                SourceEntityType: "ticket",
                SourceEntityId: ticketId,
                ActorUserId: null,
                OwnerUserId: null,
                RelatedRecords: [new ActivityRelatedRecordDto("ticket", ticketId, null, "subject")],
                Metadata: new Dictionary<string, string?>()))
            .ToList();
    }

    private async Task<IReadOnlyList<ActivityTimelineItemDto>> GetCustomerManagementEntityEntriesAsync(
        string entityName,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var timeline = await mediator.Send(
            new GetEntityTimelineQuery
            {
                EntityName = entityName,
                EntityId = entityId,
                Take = 200
            },
            cancellationToken);

        return timeline
            .Select((x, index) => ToCanonicalFromCustomerManagementTimeline(x, entityName, entityId, index))
            .ToList();
    }

    private async Task<IReadOnlyList<ActivityTimelineItemDto>> GetTaskEntriesAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await mediator.Send(new GetWorkTaskByIdQuery(taskId), cancellationToken);
        if (task is null)
        {
            return [];
        }

        var items = new List<ActivityTimelineItemDto>
        {
            new(
                Id: $"task:{task.Id}:snapshot",
                OccurredAtUtc: task.CompletedAtUtc ?? task.DueAtUtc,
                Type: "task.snapshot",
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                SourceModule: "work-management",
                SourceEntityType: "task",
                SourceEntityId: task.Id,
                ActorUserId: task.CompletedByUserId,
                OwnerUserId: task.OwnerUserId,
                RelatedRecords: [new ActivityRelatedRecordDto("task", task.Id, task.Title, "subject")],
                Metadata: new Dictionary<string, string?>
                {
                    ["priority"] = task.Priority.ToString(),
                    ["dueAtUtc"] = task.DueAtUtc.ToString("O"),
                    ["reminderAtUtc"] = task.ReminderAtUtc?.ToString("O"),
                    ["completedAtUtc"] = task.CompletedAtUtc?.ToString("O")
                })
        };

        return items;
    }

    private static ActivityTimelineItemDto ToCanonicalFromActivityLog(
        Guid id,
        DateTime occurredAtUtc,
        string type,
        string subject,
        Guid? relatedEntityId,
        string? createdBy)
    {
        var relatedRecords = relatedEntityId.HasValue
            ? new[] { new ActivityRelatedRecordDto("unknown", relatedEntityId.Value, null, "related") }
            : Array.Empty<ActivityRelatedRecordDto>();
        Guid? actorUserId = null;
        if (!string.IsNullOrWhiteSpace(createdBy) && Guid.TryParse(createdBy, out var parsedActor))
        {
            actorUserId = parsedActor;
        }

        return new ActivityTimelineItemDto(
            Id: id.ToString(),
            OccurredAtUtc: occurredAtUtc,
            Type: type.ToLowerInvariant(),
            Title: subject,
            Description: null,
            Status: null,
            SourceModule: "work-management",
            SourceEntityType: "activity-log",
            SourceEntityId: relatedEntityId,
            ActorUserId: actorUserId,
            OwnerUserId: null,
            RelatedRecords: relatedRecords,
            Metadata: new Dictionary<string, string?>());
    }

    private static ActivityTimelineItemDto ToCanonicalFromCustomerManagementTimeline(
        TimelineEventDto source,
        string entityType,
        Guid entityId,
        int index)
    {
        Guid? actorUserId = null;
        if (!string.IsNullOrWhiteSpace(source.Actor) && Guid.TryParse(source.Actor, out var parsedActor))
        {
            actorUserId = parsedActor;
        }

        return new ActivityTimelineItemDto(
            Id: $"customer-management:{entityType}:{entityId}:{index}",
            OccurredAtUtc: source.OccurredAt,
            Type: source.EventType,
            Title: source.Title,
            Description: source.Description,
            Status: null,
            SourceModule: "customer-management",
            SourceEntityType: entityType,
            SourceEntityId: entityId,
            ActorUserId: actorUserId,
            OwnerUserId: null,
            RelatedRecords: [new ActivityRelatedRecordDto(entityType, entityId, null, "subject")],
            Metadata: source.ReferenceId.HasValue
                ? new Dictionary<string, string?> { ["referenceId"] = source.ReferenceId.Value.ToString() }
                : new Dictionary<string, string?>());
    }

    private static ActivityTimelineItemDto ToCanonicalFromActivityLogForRelated(
        Guid id,
        DateTime occurredAtUtc,
        string type,
        string subject,
        string relatedEntityType,
        Guid relatedEntityId,
        string? createdBy)
    {
        Guid? actorUserId = null;
        if (!string.IsNullOrWhiteSpace(createdBy) && Guid.TryParse(createdBy, out var parsedActor))
        {
            actorUserId = parsedActor;
        }

        return new ActivityTimelineItemDto(
            Id: id.ToString(),
            OccurredAtUtc: occurredAtUtc,
            Type: type.ToLowerInvariant(),
            Title: subject,
            Description: null,
            Status: null,
            SourceModule: "work-management",
            SourceEntityType: relatedEntityType,
            SourceEntityId: relatedEntityId,
            ActorUserId: actorUserId,
            OwnerUserId: null,
            RelatedRecords: [new ActivityRelatedRecordDto(relatedEntityType, relatedEntityId, null, "primary")],
            Metadata: new Dictionary<string, string?>());
    }
}
