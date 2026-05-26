// <copyright file="ITicketAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.TicketManagement.Application.Commands.Categories;
using NetMetric.CRM.TicketManagement.Application.Commands.Tickets;
using NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkAssignTicketsOwner;
using NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkSoftDeleteTickets;
using NetMetric.CRM.TicketManagement.Application.Features.Comments.Commands.AddTicketComment;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Abstractions.Services;

public interface ITicketAdministrationService
{
    Task<TicketDetailDto> CreateAsync(CreateTicketCommand request, CancellationToken cancellationToken);
    Task<TicketDetailDto> UpdateAsync(UpdateTicketCommand request, CancellationToken cancellationToken);
    Task AssignOwnerAsync(AssignTicketOwnerCommand request, CancellationToken cancellationToken);
    Task ChangeStatusAsync(ChangeTicketStatusCommand request, CancellationToken cancellationToken);
    Task ChangePriorityAsync(ChangeTicketPriorityCommand request, CancellationToken cancellationToken);
    Task<TicketCommentDto> AddCommentAsync(AddTicketCommentCommand request, CancellationToken cancellationToken);
    Task SoftDeleteAsync(SoftDeleteTicketCommand request, CancellationToken cancellationToken);

    Task<int> BulkAssignOwnerAsync(BulkAssignTicketsOwnerCommand request, CancellationToken cancellationToken);
    Task<int> BulkSoftDeleteAsync(BulkSoftDeleteTicketsCommand request, CancellationToken cancellationToken);

    Task<TicketCategoryDto> CreateCategoryAsync(CreateTicketCategoryCommand request, CancellationToken cancellationToken);
    Task<TicketCategoryDto> UpdateCategoryAsync(UpdateTicketCategoryCommand request, CancellationToken cancellationToken);
    Task SoftDeleteCategoryAsync(SoftDeleteTicketCategoryCommand request, CancellationToken cancellationToken);
}
