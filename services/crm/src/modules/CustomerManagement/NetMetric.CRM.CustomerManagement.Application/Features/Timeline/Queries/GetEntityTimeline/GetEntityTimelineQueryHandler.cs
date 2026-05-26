// <copyright file="GetEntityTimelineQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.Auditing;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Timeline;
using NetMetric.CRM.Documents;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Timeline.Queries.GetEntityTimeline;

public sealed class GetEntityTimelineQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService) : IRequestHandler<GetEntityTimelineQuery, IReadOnlyList<TimelineEventDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<IReadOnlyList<TimelineEventDto>> Handle(GetEntityTimelineQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;
        var normalizedEntityName = request.EntityName.Trim().ToLowerInvariant();
        var events = new List<TimelineEventDto>();

        switch (normalizedEntityName)
        {
            case EntityNames.Company:
                {
                    var company = await _dbContext.Set<Company>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.EntityId, cancellationToken);
                    if (company is not null)
                    {
                        events.Add(new TimelineEventDto
                        {
                            EventType = "record.created",
                            Title = "Company created",
                            Description = company.Name,
                            OccurredAt = company.CreatedAt,
                            Actor = company.CreatedBy,
                            ReferenceId = company.Id
                        });

                        if (company.UpdatedAt.HasValue && company.UpdatedAt.Value > company.CreatedAt)
                        {
                            events.Add(new TimelineEventDto
                            {
                                EventType = "record.updated",
                                Title = "Company updated",
                                Description = company.Name,
                                OccurredAt = company.UpdatedAt.Value,
                                Actor = company.UpdatedBy,
                                ReferenceId = company.Id
                            });
                        }
                    }

                    break;
                }
            case EntityNames.Contact:
                {
                    var contact = await _dbContext.Set<Contact>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.EntityId, cancellationToken);
                    if (contact is not null)
                    {
                        events.Add(new TimelineEventDto
                        {
                            EventType = "record.created",
                            Title = "Contact created",
                            Description = $"{contact.FirstName} {contact.LastName}".Trim(),
                            OccurredAt = contact.CreatedAt,
                            Actor = contact.CreatedBy,
                            ReferenceId = contact.Id
                        });

                        if (contact.UpdatedAt.HasValue && contact.UpdatedAt.Value > contact.CreatedAt)
                        {
                            events.Add(new TimelineEventDto
                            {
                                EventType = "record.updated",
                                Title = "Contact updated",
                                Description = $"{contact.FirstName} {contact.LastName}".Trim(),
                                OccurredAt = contact.UpdatedAt.Value,
                                Actor = contact.UpdatedBy,
                                ReferenceId = contact.Id
                            });
                        }
                    }

                    break;
                }
            case EntityNames.Customer:
                {
                    var customer = await _dbContext.Set<Customer>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.EntityId, cancellationToken);
                    if (customer is not null)
                    {
                        events.Add(new TimelineEventDto
                        {
                            EventType = "record.created",
                            Title = "Customer created",
                            Description = $"{customer.FirstName} {customer.LastName}".Trim(),
                            OccurredAt = customer.CreatedAt,
                            Actor = customer.CreatedBy,
                            ReferenceId = customer.Id
                        });

                        if (customer.UpdatedAt.HasValue && customer.UpdatedAt.Value > customer.CreatedAt)
                        {
                            events.Add(new TimelineEventDto
                            {
                                EventType = "record.updated",
                                Title = "Customer updated",
                                Description = $"{customer.FirstName} {customer.LastName}".Trim(),
                                OccurredAt = customer.UpdatedAt.Value,
                                Actor = customer.UpdatedBy,
                                ReferenceId = customer.Id
                            });
                        }
                    }

                    break;
                }
            default:
                return [];
        }

        var notesQuery = _dbContext.Set<Note>().AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted).AsQueryable();
        var documentsQuery = _dbContext.Set<Document>().AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted).AsQueryable();
        IQueryable<Activity>? activitiesQuery = null;
        try
        {
            activitiesQuery = _dbContext.Set<Activity>().AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted).AsQueryable();
        }
        catch (InvalidOperationException)
        {
            activitiesQuery = null;
        }
        var auditLogsQuery = _dbContext.Set<AuditLog>().AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted).AsQueryable();

        switch (normalizedEntityName)
        {
            case EntityNames.Company:
                notesQuery = notesQuery.Where(x => x.CompanyId == request.EntityId);
                documentsQuery = documentsQuery.Where(x => x.CompanyId == request.EntityId);
                activitiesQuery = activitiesQuery?.Where(x => x.CompanyId == request.EntityId);
                auditLogsQuery = auditLogsQuery.Where(x => x.EntityId == request.EntityId && x.EntityName == nameof(Company));
                break;
            case EntityNames.Contact:
                notesQuery = notesQuery.Where(x => x.ContactId == request.EntityId);
                documentsQuery = documentsQuery.Where(x => x.ContactId == request.EntityId);
                activitiesQuery = activitiesQuery?.Where(x => x.ContactId == request.EntityId);
                auditLogsQuery = auditLogsQuery.Where(x => x.EntityId == request.EntityId && x.EntityName == nameof(Contact));
                break;
            case EntityNames.Customer:
                notesQuery = notesQuery.Where(x => x.CustomerId == request.EntityId);
                documentsQuery = documentsQuery.Where(x => x.CustomerId == request.EntityId);
                activitiesQuery = activitiesQuery?.Where(x => x.CustomerId == request.EntityId);
                auditLogsQuery = auditLogsQuery.Where(x => x.EntityId == request.EntityId && x.EntityName == nameof(Customer));
                break;
        }

        var notes = await notesQuery
            .OrderByDescending(x => x.CreatedAt)
            .Take(request.Take)
            .Select(x => new TimelineEventDto
            {
                EventType = "note.created",
                Title = x.Title,
                Description = x.Content,
                OccurredAt = x.CreatedAt,
                Actor = x.CreatedBy,
                ReferenceId = x.Id
            })
            .ToListAsync(cancellationToken);

        var documents = await documentsQuery
            .OrderByDescending(x => x.CreatedAt)
            .Take(request.Take)
            .Select(x => new TimelineEventDto
            {
                EventType = "document.attached",
                Title = x.OriginalFileName,
                Description = x.PathOrUrl,
                OccurredAt = x.CreatedAt,
                Actor = x.CreatedBy,
                ReferenceId = x.Id
            })
            .ToListAsync(cancellationToken);

        var activities = activitiesQuery is null
            ? []
            : await activitiesQuery
                .OrderByDescending(x => x.StartDate)
                .Take(request.Take)
                .Select(x => new TimelineEventDto
                {
                    EventType = "activity.scheduled",
                    Title = x.Subject,
                    Description = x.Description,
                    OccurredAt = x.StartDate ?? x.DueAt ?? x.CreatedAt,
                    Actor = x.CreatedBy,
                    ReferenceId = x.Id
                })
                .ToListAsync(cancellationToken);

        var audits = _securityService.CanReadAuditLogs()
            ? await auditLogsQuery
                .OrderByDescending(x => x.ChangedAt)
                .Take(request.Take)
                .Select(x => new TimelineEventDto
                {
                    EventType = $"audit.{x.ActionType.ToLower()}",
                    Title = $"{x.ActionType} {x.EntityName}",
                    Description = x.ChangedColumnsJson,
                    OccurredAt = x.ChangedAt,
                    Actor = x.ChangedByUserId.HasValue ? x.ChangedByUserId.Value.ToString() : x.CreatedBy,
                    ReferenceId = x.Id
                })
                .ToListAsync(cancellationToken)
            : [];

        events.AddRange(notes);
        events.AddRange(documents);
        events.AddRange(activities);
        events.AddRange(audits);

        return events
            .OrderByDescending(x => x.OccurredAt)
            .Take(request.Take)
            .ToList();
    }
}
