// <copyright file="CreateSlaPolicyCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;

public sealed record CreateSlaPolicyCommand(
    string Name,
    Guid? TicketCategoryId,
    int Priority,
    int FirstResponseTargetMinutes,
    int ResolutionTargetMinutes,
    bool IsDefault) : IRequest<Guid>
{
    public static CreateSlaPolicyCommand FromRequest(SlaPolicyUpsertRequest request) =>
        new(request.Name, request.TicketCategoryId, request.Priority, request.FirstResponseTargetMinutes, request.ResolutionTargetMinutes, request.IsDefault);
}
