// <copyright file="UpdateSlaEscalationRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.Escalations;

public sealed record UpdateSlaEscalationRuleCommand(
    Guid Id,
    Guid SlaPolicyId,
    string MetricType,
    int TriggerBeforeOrAfterMinutes,
    string ActionType,
    Guid? TargetTeamId,
    Guid? TargetUserId,
    bool IsEnabled) : IRequest
{
    public static UpdateSlaEscalationRuleCommand FromRequest(Guid id, SlaEscalationRuleUpsertRequest request) =>
        new(id, request.SlaPolicyId, request.MetricType, request.TriggerBeforeOrAfterMinutes, request.ActionType, request.TargetTeamId, request.TargetUserId, request.IsEnabled);
}
