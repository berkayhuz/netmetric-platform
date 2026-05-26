// <copyright file="ActivityTimelineWriteService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.API.Contracts.Activities;
using NetMetric.CRM.CustomerManagement.Application.Queries.Companies;
using NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;
using NetMetric.CRM.CustomerManagement.Application.Queries.Customers;
using NetMetric.CRM.DealManagement.Application.Queries.Deals;
using NetMetric.CRM.LeadManagement.Application.Queries.Leads;
using NetMetric.CRM.OpportunityManagement.Application.Queries.Opportunities;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.TicketManagement.Application.Queries.Tickets;
using NetMetric.CRM.WorkManagement.Application.Commands.Meetings.ScheduleMeeting;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CreateWorkTask;
using NetMetric.CRM.WorkManagement.Domain.Entities;
using NetMetric.CRM.WorkManagement.Domain.Enums;
using NetMetric.CRM.WorkManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.API.Features.Activities;

public interface IActivityTimelineWriteService
{
    Task<CreateActivityResponseDto> CreateAsync(CreateActivityRequestDto request, CancellationToken cancellationToken);
}

public sealed class ActivityValidationException(string key, string message) : Exception(message)
{
    public string Key { get; } = key;
}

public sealed class ActivityTimelineWriteService(
    WorkManagementDbContext workManagementDbContext,
    IMediator mediator,
    ICurrentUserService currentUserService) : IActivityTimelineWriteService
{
    public async Task<CreateActivityResponseDto> CreateAsync(CreateActivityRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedType = request.Type.Trim().ToLowerInvariant();
        var relatedRecords = ValidateRelatedRecords(request.RelatedRecords);
        await ValidateRelatedRecordExistenceAsync(relatedRecords, cancellationToken);

        var activityType = normalizedType switch
        {
            "note" => WorkActivityType.Note,
            "call" => WorkActivityType.Call,
            "email" => WorkActivityType.Email,
            "task" => WorkActivityType.Task,
            "meeting" => WorkActivityType.Meeting,
            _ => throw new ActivityValidationException(
                "type",
                "Unsupported type. Supported values: note, call, email, task, meeting.")
        };

        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        if (activityType is WorkActivityType.Task or WorkActivityType.Meeting &&
            !currentUserService.HasPermission("work-management.manage"))
        {
            throw new UnauthorizedAccessException("work-management.manage permission is required for task and meeting activity creation.");
        }

        var occurredAtUtc = request.OccurredAtUtc ?? DateTime.UtcNow;
        var subject = activityType switch
        {
            WorkActivityType.Note => BuildNoteSubject(request),
            WorkActivityType.Call => BuildCallSubject(request),
            WorkActivityType.Email => BuildEmailSubject(request),
            WorkActivityType.Task => BuildTaskSubject(request),
            WorkActivityType.Meeting => BuildMeetingSubject(request),
            _ => throw new InvalidOperationException("Unsupported activity type.")
        };

        var primary = relatedRecords.Single(x => string.Equals(x.RelationRole, "primary", StringComparison.OrdinalIgnoreCase));

        var (sourceEntityType, sourceEntityId) = activityType switch
        {
            WorkActivityType.Task => await CreateTaskAsync(request, cancellationToken),
            WorkActivityType.Meeting => await CreateMeetingAsync(request, cancellationToken),
            _ => (primary.EntityType.Trim().ToLowerInvariant(), primary.EntityId)
        };

        var activity = new ActivityLog(activityType, subject, occurredAtUtc, primary.EntityId);
        workManagementDbContext.Activities.Add(activity);
        await workManagementDbContext.SaveChangesAsync(cancellationToken);

        var timelineItem = new ActivityTimelineItemDto(
            Id: activity.Id.ToString(),
            OccurredAtUtc: activity.OccurredAtUtc,
            Type: normalizedType,
            Title: activity.Subject,
            Description: request.Description?.Trim(),
            Status: null,
            SourceModule: "work-management",
            SourceEntityType: sourceEntityType,
            SourceEntityId: sourceEntityId,
            ActorUserId: currentUserService.UserId,
            OwnerUserId: null,
            RelatedRecords: relatedRecords.Select(x => new ActivityRelatedRecordDto(
                x.EntityType.Trim().ToLowerInvariant(),
                x.EntityId,
                null,
                x.RelationRole.Trim().ToLowerInvariant())).ToArray(),
            Metadata: BuildMetadata(normalizedType, request));

        return new CreateActivityResponseDto(
            ActivityId: activity.Id,
            Type: normalizedType,
            CreatedAtUtc: activity.CreatedAt == default ? DateTime.UtcNow : activity.CreatedAt,
            SourceEntityType: sourceEntityType,
            SourceEntityId: sourceEntityId,
            TimelineItem: timelineItem);
    }

    private async Task<(string SourceEntityType, Guid SourceEntityId)> CreateTaskAsync(
        CreateActivityRequestDto request,
        CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ActivityValidationException("title", "Title is required for task.");
        }

        var details = payload?.Details?.Trim() ?? request.Description?.Trim() ?? string.Empty;
        var dueAtUtc = payload?.DueAtUtc ?? DateTime.UtcNow.AddDays(1);
        var priority = ParseTaskPriority(payload?.Priority);

        var task = await mediator.Send(
            new CreateWorkTaskCommand(
                title,
                details,
                payload?.OwnerUserId,
                dueAtUtc,
                priority),
            cancellationToken);

        return ("task", task.Id);
    }

    private async Task<(string SourceEntityType, Guid SourceEntityId)> CreateMeetingAsync(
        CreateActivityRequestDto request,
        CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ActivityValidationException("title", "Title is required for meeting.");
        }

        var startsAtUtc = payload?.StartUtc
            ?? throw new ActivityValidationException("payload.startUtc", "StartUtc is required for meeting.");
        var endsAtUtc = payload?.EndUtc
            ?? throw new ActivityValidationException("payload.endUtc", "EndUtc is required for meeting.");

        if (startsAtUtc >= endsAtUtc)
        {
            throw new ActivityValidationException("payload.endUtc", "EndUtc must be greater than StartUtc for meeting.");
        }

        var organizerEmail = currentUserService.Email?.Trim();
        if (string.IsNullOrWhiteSpace(organizerEmail))
        {
            throw new ActivityValidationException("user.email", "Organizer email is required for meeting scheduling.");
        }

        var attendeeSummary = payload?.AttendeeUserIds is { Count: > 0 }
            ? string.Join(',', payload.AttendeeUserIds)
            : payload?.Location?.Trim() ?? request.Description?.Trim() ?? string.Empty;

        var meeting = await mediator.Send(
            new ScheduleMeetingCommand(
                title,
                startsAtUtc,
                endsAtUtc,
                organizerEmail,
                attendeeSummary,
                RequiresExternalSync: false),
            cancellationToken);

        return ("meeting", meeting.Id);
    }

    private static IReadOnlyDictionary<string, string?> BuildMetadata(string normalizedType, CreateActivityRequestDto request)
    {
        var payload = request.Payload;
        var metadata = new Dictionary<string, string?>();
        switch (normalizedType)
        {
            case "call":
                metadata["direction"] = payload?.Direction?.Trim().ToLowerInvariant();
                metadata["outcome"] = payload?.Outcome?.Trim().ToLowerInvariant();
                metadata["durationSeconds"] = payload?.DurationSeconds?.ToString();
                break;
            case "email":
                metadata["direction"] = payload?.Direction?.Trim().ToLowerInvariant();
                metadata["subject"] = payload?.Subject?.Trim();
                metadata["toCount"] = payload?.To?.Count.ToString();
                metadata["ccCount"] = payload?.Cc?.Count.ToString();
                break;
            case "task":
                metadata["priority"] = payload?.Priority?.Trim().ToLowerInvariant();
                metadata["dueAtUtc"] = payload?.DueAtUtc?.ToString("O");
                metadata["ownerUserId"] = payload?.OwnerUserId?.ToString();
                break;
            case "meeting":
                metadata["startUtc"] = payload?.StartUtc?.ToString("O");
                metadata["endUtc"] = payload?.EndUtc?.ToString("O");
                metadata["location"] = payload?.Location?.Trim();
                metadata["attendeeCount"] = payload?.AttendeeUserIds?.Count.ToString();
                break;
        }

        return metadata;
    }

    private static string BuildNoteSubject(CreateActivityRequestDto request)
    {
        var body = request.Payload?.Body?.Trim();
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ActivityValidationException("payload.body", "Body is required for note.");
        }

        var title = request.Title?.Trim();
        return TruncateSubject(string.IsNullOrWhiteSpace(title) ? body : title);
    }

    private static string BuildCallSubject(CreateActivityRequestDto request)
    {
        var payload = request.Payload;
        var direction = payload?.Direction?.Trim().ToLowerInvariant();
        if (direction is not ("inbound" or "outbound"))
        {
            throw new ActivityValidationException("payload.direction", "Direction is required and must be inbound or outbound for call.");
        }

        var outcome = payload?.Outcome?.Trim().ToLowerInvariant();
        if (outcome is not ("connected" or "no_answer" or "voicemail" or "other"))
        {
            throw new ActivityValidationException("payload.outcome", "Outcome is required and must be connected, no_answer, voicemail or other for call.");
        }

        if (payload?.DurationSeconds is < 0)
        {
            throw new ActivityValidationException("payload.durationSeconds", "DurationSeconds must be greater than or equal to 0 for call.");
        }

        var summary = payload?.Summary?.Trim();
        var title = request.Title?.Trim();
        var fallback = $"Call ({direction}) - {outcome}";
        return TruncateSubject(!string.IsNullOrWhiteSpace(title) ? title : !string.IsNullOrWhiteSpace(summary) ? summary : fallback);
    }

    private static string BuildEmailSubject(CreateActivityRequestDto request)
    {
        var payload = request.Payload;
        var subject = payload?.Subject?.Trim();
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ActivityValidationException("payload.subject", "Subject is required for email.");
        }

        var bodySummary = payload?.BodySummary?.Trim();
        if (string.IsNullOrWhiteSpace(bodySummary))
        {
            throw new ActivityValidationException("payload.bodySummary", "BodySummary is required for email.");
        }

        var direction = payload?.Direction?.Trim().ToLowerInvariant();
        if (direction is not ("inbound" or "outbound"))
        {
            throw new ActivityValidationException("payload.direction", "Direction is required and must be inbound or outbound for email.");
        }

        return TruncateSubject(subject);
    }

    private static string BuildTaskSubject(CreateActivityRequestDto request)
    {
        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ActivityValidationException("title", "Title is required for task.");
        }

        return TruncateSubject(title);
    }

    private static string BuildMeetingSubject(CreateActivityRequestDto request)
    {
        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ActivityValidationException("title", "Title is required for meeting.");
        }

        return TruncateSubject(title);
    }

    private static int ParseTaskPriority(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized switch
        {
            null or "" => 3,
            "low" => 2,
            "normal" => 3,
            "high" => 4,
            _ => throw new ActivityValidationException("payload.priority", "Priority must be low, normal or high for task.")
        };
    }

    private static string TruncateSubject(string subject)
        => subject.Length <= 180 ? subject : subject[..180];

    private static IReadOnlyList<CreateActivityRelatedRecordDto> ValidateRelatedRecords(IReadOnlyList<CreateActivityRelatedRecordDto>? relatedRecords)
    {
        if (relatedRecords is null || relatedRecords.Count == 0)
        {
            throw new ActivityValidationException("relatedRecords", "At least one related record is required.");
        }

        var primaryCount = relatedRecords.Count(x =>
            string.Equals(x.RelationRole, "primary", StringComparison.OrdinalIgnoreCase));
        if (primaryCount != 1)
        {
            throw new ActivityValidationException("relatedRecords", "Exactly one primary related record is required.");
        }

        foreach (var relatedRecord in relatedRecords)
        {
            if (!IsSupportedEntityType(relatedRecord.EntityType))
            {
                throw new ActivityValidationException("relatedRecords.entityType", "Unsupported entity type. Supported values: lead, opportunity, deal, quote, ticket, customer, company, contact.");
            }
        }

        return relatedRecords;
    }

    private static bool IsSupportedEntityType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "lead" or "opportunity" or "deal" or "quote" or "ticket" or "customer" or "company" or "contact";
    }

    private async Task ValidateRelatedRecordExistenceAsync(
        IReadOnlyList<CreateActivityRelatedRecordDto> relatedRecords,
        CancellationToken cancellationToken)
    {
        foreach (var relatedRecord in relatedRecords)
        {
            var normalizedType = relatedRecord.EntityType.Trim().ToLowerInvariant();
            var exists = normalizedType switch
            {
                "lead" => await mediator.Send(new GetLeadByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                "opportunity" => await mediator.Send(new GetOpportunityByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                "deal" => await mediator.Send(new GetDealByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                "quote" => await mediator.Send(new GetQuoteByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                "ticket" => await mediator.Send(new GetTicketByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                "customer" => await mediator.Send(new GetCustomerByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                "company" => await mediator.Send(new GetCompanyByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                "contact" => await mediator.Send(new GetContactByIdQuery(relatedRecord.EntityId), cancellationToken) is not null,
                _ => false
            };

            if (!exists)
            {
                throw new KeyNotFoundException($"Related {relatedRecord.EntityType} record was not found.");
            }
        }
    }
}
