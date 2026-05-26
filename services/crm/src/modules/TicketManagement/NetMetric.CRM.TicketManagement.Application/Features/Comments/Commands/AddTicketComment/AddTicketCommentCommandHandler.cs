// <copyright file="AddTicketCommentCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Features.Comments.Commands.AddTicketComment;

public sealed class AddTicketCommentCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<AddTicketCommentCommand, TicketCommentDto>
{
    public Task<TicketCommentDto> Handle(AddTicketCommentCommand request, CancellationToken cancellationToken)
        => administrationService.AddCommentAsync(request, cancellationToken);
}
