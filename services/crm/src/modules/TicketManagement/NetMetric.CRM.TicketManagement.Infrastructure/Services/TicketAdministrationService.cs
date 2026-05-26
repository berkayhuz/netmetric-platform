// <copyright file="TicketAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NetMetric.Clock;
using NetMetric.CRM.Core;
using NetMetric.CRM.Support;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Integration;
using NetMetric.CRM.TicketManagement.Application.Commands.Categories;
using NetMetric.CRM.TicketManagement.Application.Commands.Tickets;
using NetMetric.CRM.TicketManagement.Application.Common;
using NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkAssignTicketsOwner;
using NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkSoftDeleteTickets;
using NetMetric.CRM.TicketManagement.Application.Features.Comments.Commands.AddTicketComment;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;
using NetMetric.CRM.TicketManagement.Infrastructure.Persistence;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Services;

public sealed class TicketAdministrationService(
    TicketManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IClock clock,
    ITicketManagementOutbox outbox) : ITicketAdministrationService
{
    public async Task<TicketDetailDto> CreateAsync(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        await ValidateReferencesAsync(
            request.CustomerId,
            request.ContactId,
            request.TicketCategoryId,
            request.SlaPolicyId,
            cancellationToken);

        var ticket = new Ticket
        {
            TenantId = currentUserService.TenantId,
            TicketNumber = TicketManagementMappingExtensions.GenerateTicketNumber(),
            Subject = request.Subject.Trim(),
            Description = Normalize(request.Description),
            TicketType = request.TicketType,
            Channel = request.Channel,
            Priority = request.Priority,
            AssignedUserId = request.AssignedUserId,
            CustomerId = request.CustomerId,
            ContactId = request.ContactId,
            TicketCategoryId = request.TicketCategoryId,
            SlaPolicyId = request.SlaPolicyId,
            FirstResponseDueAt = request.FirstResponseDueAt,
            ResolveDueAt = request.ResolveDueAt,
            OpenedAt = clock.UtcDateTime,
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = ResolveActor(),
            UpdatedBy = ResolveActor()
        };

        ticket.SetNotes(request.Notes);

        dbContext.Tickets.Add(ticket);
        await outbox.EnqueueTicketCreatedAsync(ticket, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }

    public async Task<TicketDetailDto> UpdateAsync(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var ticket = await GetTicketForWriteAsync(request.TicketId, cancellationToken);
        ConcurrencyHelper.ApplyRowVersion(dbContext, ticket, request.RowVersion);
        await ValidateReferencesAsync(
            request.CustomerId,
            request.ContactId,
            request.TicketCategoryId,
            request.SlaPolicyId,
            cancellationToken);

        ticket.Subject = request.Subject.Trim();
        ticket.Description = Normalize(request.Description);
        ticket.TicketType = request.TicketType;
        ticket.Channel = request.Channel;
        ticket.Priority = request.Priority;
        ticket.AssignedUserId = request.AssignedUserId;
        ticket.CustomerId = request.CustomerId;
        ticket.ContactId = request.ContactId;
        ticket.TicketCategoryId = request.TicketCategoryId;
        ticket.SlaPolicyId = request.SlaPolicyId;
        ticket.FirstResponseDueAt = request.FirstResponseDueAt;
        ticket.ResolveDueAt = request.ResolveDueAt;
        ticket.UpdatedAt = clock.UtcDateTime;
        ticket.UpdatedBy = ResolveActor();
        ticket.SetNotes(request.Notes);

        await outbox.EnqueueTicketUpdatedAsync(ticket, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(ticket).Collection(x => x.Comments).LoadAsync(cancellationToken);
        return ticket.ToDetailDto();
    }

    public async Task AssignOwnerAsync(AssignTicketOwnerCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var ticket = await GetTicketForWriteAsync(request.TicketId, cancellationToken);
        ticket.AssignedUserId = request.OwnerUserId;
        ticket.UpdatedAt = clock.UtcDateTime;
        ticket.UpdatedBy = ResolveActor();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeStatusAsync(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var ticket = await GetTicketForWriteAsync(request.TicketId, cancellationToken);

        if (ticket.Status == request.Status)
            return;

        var oldStatus = ticket.Status;
        ticket.Status = request.Status;
        ticket.UpdatedAt = clock.UtcDateTime;
        ticket.UpdatedBy = ResolveActor();

        if (request.Status is TicketStatusType.Resolved or TicketStatusType.Closed)
        {
            ticket.ClosedAt = clock.UtcDateTime;
            ticket.Deactivate();
        }
        else
        {
            ticket.ClosedAt = null;
            ticket.Activate();
        }

        dbContext.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            OldStatus = oldStatus,
            NewStatus = request.Status,
            ChangedAt = clock.UtcDateTime,
            ChangedByUserId = currentUserService.UserId,
            Note = Normalize(request.Note),
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = ResolveActor(),
            UpdatedBy = ResolveActor()
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePriorityAsync(ChangeTicketPriorityCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var ticket = await GetTicketForWriteAsync(request.TicketId, cancellationToken);

        if (ticket.Priority == request.Priority)
            return;

        var oldPriority = ticket.Priority;
        ticket.Priority = request.Priority;
        ticket.UpdatedAt = clock.UtcDateTime;
        ticket.UpdatedBy = ResolveActor();

        dbContext.TicketPriorityHistories.Add(new TicketPriorityHistory
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            OldPriority = oldPriority,
            NewPriority = request.Priority,
            ChangedAt = clock.UtcDateTime,
            ChangedByUserId = currentUserService.UserId,
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = ResolveActor(),
            UpdatedBy = ResolveActor()
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TicketCommentDto> AddCommentAsync(AddTicketCommentCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var ticket = await GetTicketForWriteAsync(request.TicketId, cancellationToken);

        var comment = new TicketComment
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            Comment = request.Comment.Trim(),
            IsInternal = request.IsInternal,
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = ResolveActor(),
            UpdatedBy = ResolveActor()
        };

        dbContext.TicketComments.Add(comment);
        ticket.UpdatedAt = clock.UtcDateTime;
        ticket.UpdatedBy = ResolveActor();

        await dbContext.SaveChangesAsync(cancellationToken);
        return comment.ToDto();
    }

    public async Task SoftDeleteAsync(SoftDeleteTicketCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var ticket = await GetTicketForWriteAsync(request.TicketId, cancellationToken);
        await AddTicketTrashItemIfMissingAsync(ticket, cancellationToken);
        dbContext.Tickets.Remove(ticket);
        await outbox.EnqueueTicketDeletedAsync(ticket, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkAssignOwnerAsync(BulkAssignTicketsOwnerCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tickets = await dbContext.Tickets
            .Where(x => x.TenantId == currentUserService.TenantId && request.TicketIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var ticket in tickets)
        {
            ticket.AssignedUserId = request.OwnerUserId;
            ticket.UpdatedAt = clock.UtcDateTime;
            ticket.UpdatedBy = ResolveActor();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return tickets.Count;
    }

    public async Task<int> BulkSoftDeleteAsync(BulkSoftDeleteTicketsCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tickets = await dbContext.Tickets
            .Where(x => x.TenantId == currentUserService.TenantId && request.TicketIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var ticket in tickets)
        {
            await AddTicketTrashItemIfMissingAsync(ticket, cancellationToken);
        }

        dbContext.Tickets.RemoveRange(tickets);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tickets.Count;
    }

    public async Task<TicketCategoryDto> CreateCategoryAsync(CreateTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var category = new TicketCategory
        {
            TenantId = currentUserService.TenantId,
            Name = request.Name.Trim(),
            Description = Normalize(request.Description),
            ParentCategoryId = request.ParentCategoryId,
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = ResolveActor(),
            UpdatedBy = ResolveActor()
        };

        dbContext.TicketCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    public async Task<TicketCategoryDto> UpdateCategoryAsync(UpdateTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var category = await dbContext.TicketCategories
            .FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.TicketCategoryId, cancellationToken)
            ?? throw new NotFoundAppException("Ticket category not found.");

        category.Name = request.Name.Trim();
        category.Description = Normalize(request.Description);
        category.ParentCategoryId = request.ParentCategoryId;
        category.UpdatedAt = clock.UtcDateTime;
        category.UpdatedBy = ResolveActor();

        await dbContext.SaveChangesAsync(cancellationToken);
        return category.ToDto();
    }

    public async Task SoftDeleteCategoryAsync(SoftDeleteTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var category = await dbContext.TicketCategories
            .FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.TicketCategoryId, cancellationToken)
            ?? throw new NotFoundAppException("Ticket category not found.");

        dbContext.TicketCategories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Ticket> GetTicketForWriteAsync(Guid ticketId, CancellationToken cancellationToken)
        => await dbContext.Tickets
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == ticketId, cancellationToken)
           ?? throw new NotFoundAppException("Ticket not found.");

    private async Task ValidateReferencesAsync(
        Guid? customerId,
        Guid? contactId,
        Guid? ticketCategoryId,
        Guid? slaPolicyId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (customerId.HasValue)
        {
            var customerExists = await dbContext.Set<Customer>()
                .AsNoTracking()
                .AnyAsync(x => x.TenantId == currentUserService.TenantId && x.Id == customerId.Value && !x.IsDeleted, cancellationToken);

            if (!customerExists)
            {
                errors[nameof(customerId)] = ["Unknown customer."];
            }
        }

        if (contactId.HasValue)
        {
            var contactExists = await dbContext.Set<Contact>()
                .AsNoTracking()
                .AnyAsync(x => x.TenantId == currentUserService.TenantId && x.Id == contactId.Value && !x.IsDeleted, cancellationToken);

            if (!contactExists)
            {
                errors[nameof(contactId)] = ["Unknown contact."];
            }
        }

        if (ticketCategoryId.HasValue)
        {
            var categoryExists = await dbContext.TicketCategories
                .AsNoTracking()
                .AnyAsync(x => x.TenantId == currentUserService.TenantId && x.Id == ticketCategoryId.Value, cancellationToken);

            if (!categoryExists)
            {
                errors[nameof(ticketCategoryId)] = ["Unknown ticket category."];
            }
        }

        if (slaPolicyId.HasValue)
        {
            var slaPolicyExists = await dbContext.SlaPolicies
                .AsNoTracking()
                .AnyAsync(x => x.TenantId == currentUserService.TenantId && x.Id == slaPolicyId.Value, cancellationToken);

            if (!slaPolicyExists)
            {
                errors[nameof(slaPolicyId)] = ["Unknown SLA policy."];
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("One or more ticket references are invalid.", errors);
        }
    }

    private string ResolveActor()
        => currentUserService.UserName ?? currentUserService.Email ?? currentUserService.UserId.ToString("N");

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task AddTicketTrashItemIfMissingAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        var exists = dbContext.ChangeTracker.Entries<GlobalTrashItem>()
            .Any(x =>
                x.State != EntityState.Deleted
                && x.Entity.TenantId == ticket.TenantId
                && x.Entity.EntityType == CrmTrashEntityTypes.Ticket
                && x.Entity.EntityId == ticket.Id
                && x.Entity.Status == CrmTrashStatuses.Active);

        if (!exists)
        {
            exists = await dbContext.Set<GlobalTrashItem>()
                .AnyAsync(
                    x => x.TenantId == ticket.TenantId
                         && x.EntityType == CrmTrashEntityTypes.Ticket
                         && x.EntityId == ticket.Id
                         && x.Status == CrmTrashStatuses.Active,
                    cancellationToken);
        }

        if (exists)
        {
            return;
        }

        var deletedAtUtc = DateTime.UtcNow;
        var displayName = !string.IsNullOrWhiteSpace(ticket.Subject)
            ? ticket.Subject.Trim()
            : !string.IsNullOrWhiteSpace(ticket.TicketNumber)
                ? ticket.TicketNumber.Trim()
                : "Deleted ticket";
        var summary = !string.IsNullOrWhiteSpace(ticket.TicketNumber) ? ticket.TicketNumber.Trim() : null;

        await dbContext.Set<GlobalTrashItem>().AddAsync(
            new GlobalTrashItem
            {
                TenantId = ticket.TenantId,
                EntityType = CrmTrashEntityTypes.Ticket,
                EntityId = ticket.Id,
                DisplayName = displayName,
                Summary = summary,
                SourceModule = "tickets",
                OriginalRoute = $"/tickets/{ticket.Id:D}",
                DeletedAtUtc = deletedAtUtc,
                DeletedByUserId = currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId,
                DeletedByDisplayName = currentUserService.UserName ?? currentUserService.Email,
                ExpiresAtUtc = deletedAtUtc.AddDays(7),
                Status = CrmTrashStatuses.Active,
                AuditCorrelationId = Activity.Current?.TraceId.ToString()
            },
            cancellationToken);
    }
}
