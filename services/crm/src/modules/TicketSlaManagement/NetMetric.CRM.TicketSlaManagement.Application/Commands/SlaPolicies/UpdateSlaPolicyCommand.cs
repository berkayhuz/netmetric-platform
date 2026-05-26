// <copyright file="UpdateSlaPolicyCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;

public sealed record UpdateSlaPolicyCommand(
    Guid Id,
    string Name,
    Guid? TicketCategoryId,
    int Priority,
    int FirstResponseTargetMinutes,
    int ResolutionTargetMinutes,
    bool IsDefault) : IRequest
{
    public static UpdateSlaPolicyCommand FromRequest(Guid id, SlaPolicyUpsertRequest request) =>
        new(id, request.Name, request.TicketCategoryId, request.Priority, request.FirstResponseTargetMinutes, request.ResolutionTargetMinutes, request.IsDefault);
}
