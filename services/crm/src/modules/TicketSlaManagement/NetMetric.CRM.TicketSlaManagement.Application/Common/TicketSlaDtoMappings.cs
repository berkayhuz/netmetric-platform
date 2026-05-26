// <copyright file="TicketSlaDtoMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ServiceManagement;
using NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;

namespace NetMetric.CRM.TicketSlaManagement.Application.Common;

public static class TicketSlaDtoMappings
{
    public static SlaPolicyListItemDto ToDto(this SlaPolicy entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        TicketCategoryId = entity.TicketCategoryId,
        Priority = entity.Priority,
        FirstResponseTargetMinutes = entity.FirstResponseTargetMinutes,
        ResolutionTargetMinutes = entity.ResolutionTargetMinutes,
        IsDefault = entity.IsDefault
    };

    public static SlaEscalationRuleDto ToDto(this SlaEscalationRule entity) => new()
    {
        Id = entity.Id,
        SlaPolicyId = entity.SlaPolicyId,
        MetricType = entity.MetricType.ToString(),
        TriggerBeforeOrAfterMinutes = entity.TriggerBeforeOrAfterMinutes,
        ActionType = entity.ActionType.ToString(),
        TargetTeamId = entity.TargetTeamId,
        TargetUserId = entity.TargetUserId,
        IsEnabled = entity.IsEnabled
    };

    public static TicketSlaWorkspaceDto ToDto(this TicketSlaInstance entity) => new()
    {
        TicketId = entity.TicketId,
        SlaPolicyId = entity.SlaPolicyId,
        FirstResponseDueAtUtc = entity.FirstResponseDueAtUtc,
        ResolutionDueAtUtc = entity.ResolutionDueAtUtc,
        FirstRespondedAtUtc = entity.FirstRespondedAtUtc,
        ResolvedAtUtc = entity.ResolvedAtUtc,
        IsFirstResponseBreached = entity.IsFirstResponseBreached,
        IsResolutionBreached = entity.IsResolutionBreached
    };

    public static TicketEscalationRunDto ToDto(this TicketEscalationRun entity) => new()
    {
        Id = entity.Id,
        TicketId = entity.TicketId,
        EscalationRuleId = entity.EscalationRuleId,
        MetricType = entity.MetricType.ToString(),
        ExecutedAtUtc = entity.ExecutedAtUtc,
        Note = entity.Note
    };
}
