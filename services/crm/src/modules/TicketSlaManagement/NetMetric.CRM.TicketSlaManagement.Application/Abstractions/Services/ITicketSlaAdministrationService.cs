// <copyright file="ITicketSlaAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ServiceManagement;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;

namespace NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

public interface ITicketSlaAdministrationService
{
    Task<Guid> CreatePolicyAsync(SlaPolicy policy, CancellationToken cancellationToken);
    Task UpdatePolicyAsync(Guid id, string name, Guid? ticketCategoryId, int priority, int firstResponseTargetMinutes, int resolutionTargetMinutes, bool isDefault, CancellationToken cancellationToken);
    Task SoftDeletePolicyAsync(Guid id, CancellationToken cancellationToken);

    Task<Guid> CreateEscalationRuleAsync(SlaEscalationRule rule, CancellationToken cancellationToken);
    Task UpdateEscalationRuleAsync(Guid id, Guid slaPolicyId, string metricType, int triggerBeforeOrAfterMinutes, string actionType, Guid? targetTeamId, Guid? targetUserId, bool isEnabled, CancellationToken cancellationToken);

    Task AttachPolicyToTicketAsync(Guid ticketId, Guid slaPolicyId, DateTime createdAtUtc, CancellationToken cancellationToken);
    Task MarkFirstResponseAsync(Guid ticketId, DateTime respondedAtUtc, CancellationToken cancellationToken);
    Task MarkResolvedAsync(Guid ticketId, DateTime resolvedAtUtc, CancellationToken cancellationToken);
    Task<int> RunDueEscalationsAsync(DateTime utcNow, CancellationToken cancellationToken);
}
