// <copyright file="CreateSlaEscalationRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.Requests;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;
using NetMetric.CRM.TicketSlaManagement.Domain.Enums;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.Escalations;

public sealed record CreateSlaEscalationRuleCommand(
    Guid SlaPolicyId,
    string MetricType,
    int TriggerBeforeOrAfterMinutes,
    string ActionType,
    Guid? TargetTeamId,
    Guid? TargetUserId,
    bool IsEnabled) : IRequest<Guid>
{
    public static CreateSlaEscalationRuleCommand FromRequest(SlaEscalationRuleUpsertRequest request) =>
        new(request.SlaPolicyId, request.MetricType, request.TriggerBeforeOrAfterMinutes, request.ActionType, request.TargetTeamId, request.TargetUserId, request.IsEnabled);

    public SlaEscalationRule ToEntity() =>
        new(
            SlaPolicyId,
            Enum.Parse<SlaMetricType>(MetricType, true),
            TriggerBeforeOrAfterMinutes,
            Enum.Parse<SlaBreachActionType>(ActionType, true),
            TargetTeamId,
            TargetUserId,
            IsEnabled);
}
