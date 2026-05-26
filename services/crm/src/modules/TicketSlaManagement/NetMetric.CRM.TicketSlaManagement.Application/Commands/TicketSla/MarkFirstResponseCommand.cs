// <copyright file="MarkFirstResponseCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

public sealed record MarkFirstResponseCommand(Guid TicketId, DateTime RespondedAtUtc) : IRequest
{
    public static MarkFirstResponseCommand FromRequest(MarkFirstResponseRequest request) => new(request.TicketId, request.RespondedAtUtc);
}
