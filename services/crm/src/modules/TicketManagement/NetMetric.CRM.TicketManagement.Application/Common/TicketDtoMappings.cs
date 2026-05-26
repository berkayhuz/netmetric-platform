// <copyright file="TicketDtoMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Support;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Common;

public static class TicketDtoMappings
{
    public static TicketListItemDto ToListItemDto(this Ticket ticket)
        => new(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Subject,
            ticket.Status,
            ticket.Priority,
            ticket.TicketType,
            ticket.AssignedUserId,
            ticket.CustomerId,
            ticket.ContactId,
            ticket.OpenedAt,
            ticket.ClosedAt,
            ticket.IsActive);

    public static TicketCommentDto ToDto(this TicketComment comment)
        => new(
            comment.Id,
            comment.Comment,
            comment.IsInternal,
            comment.CreatedAt,
            comment.CreatedBy);

    public static TicketDetailDto ToDetailDto(this Ticket ticket, bool includeInternalNotes = true)
        => new(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Subject,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.TicketType,
            ticket.Channel,
            ticket.AssignedUserId,
            ticket.CustomerId,
            ticket.ContactId,
            ticket.TicketCategoryId,
            ticket.SlaPolicyId,
            ticket.OpenedAt,
            ticket.ClosedAt,
            ticket.FirstResponseDueAt,
            ticket.ResolveDueAt,
            includeInternalNotes ? ticket.Notes : null,
            ticket.IsActive,
            ticket.RowVersion,
            ticket.Comments
                .Where(x => includeInternalNotes || !x.IsInternal)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ToDto())
                .ToList());

    public static TicketCategoryDto ToDto(this TicketCategory category)
        => new(category.Id, category.Name, category.Description, category.ParentCategoryId, category.IsActive);
}
