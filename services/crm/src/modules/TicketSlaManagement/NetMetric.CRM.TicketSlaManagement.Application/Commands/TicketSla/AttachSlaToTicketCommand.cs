// <copyright file="AttachSlaToTicketCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

public sealed record AttachSlaToTicketCommand(Guid TicketId, Guid SlaPolicyId, DateTime CreatedAtUtc) : IRequest
{
    public static AttachSlaToTicketCommand FromRequest(AttachSlaToTicketRequest request) =>
        new(request.TicketId, request.SlaPolicyId, request.CreatedAtUtc);
}
