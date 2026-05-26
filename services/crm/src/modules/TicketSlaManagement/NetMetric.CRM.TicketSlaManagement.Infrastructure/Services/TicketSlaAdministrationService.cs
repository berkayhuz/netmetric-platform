// <copyright file="TicketSlaAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Clock;
using NetMetric.CRM.ServiceManagement;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;
using NetMetric.CRM.TicketSlaManagement.Domain.Enums;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TicketSlaManagement.Infrastructure.Services;

public sealed class TicketSlaAdministrationService(
    ITicketSlaManagementDbContext dbContext,
    ITenantContext tenantContext,
    IClock clock) : ITicketSlaAdministrationService
{
    public async Task<Guid> CreatePolicyAsync(SlaPolicy policy, CancellationToken cancellationToken)
    {
        policy.TenantId = tenantContext.GetRequiredTenantId();

        if (policy.IsDefault)
            await ClearDefaultsAsync(policy.Priority, cancellationToken);

        await dbContext.SlaPolicies.AddAsync(policy, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return policy.Id;
    }

    public async Task UpdatePolicyAsync(Guid id, string name, Guid? ticketCategoryId, int priority, int firstResponseTargetMinutes, int resolutionTargetMinutes, bool isDefault, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SlaPolicies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundAppException("SLA policy was not found.");

        entity.Rename(name);
        entity.RebindCategory(ticketCategoryId);
        entity.SetPriority(priority);
        entity.SetTargets(firstResponseTargetMinutes, resolutionTargetMinutes);

        if (isDefault)
        {
            await ClearDefaultsAsync(priority, cancellationToken);
            entity.MarkAsDefault();
        }
        else
        {
            entity.RemoveDefault();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeletePolicyAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SlaPolicies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundAppException("SLA policy was not found.");

        entity.IsDeleted = true;
        entity.DeletedAt = clock.UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateEscalationRuleAsync(SlaEscalationRule rule, CancellationToken cancellationToken)
    {
        rule.TenantId = tenantContext.GetRequiredTenantId();
        await dbContext.SlaEscalationRules.AddAsync(rule, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }

    public async Task UpdateEscalationRuleAsync(Guid id, Guid slaPolicyId, string metricType, int triggerBeforeOrAfterMinutes, string actionType, Guid? targetTeamId, Guid? targetUserId, bool isEnabled, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SlaEscalationRules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundAppException("Escalation rule was not found.");

        var updated = new SlaEscalationRule(
            slaPolicyId,
            Enum.Parse<SlaMetricType>(metricType, true),
            triggerBeforeOrAfterMinutes,
            Enum.Parse<SlaBreachActionType>(actionType, true),
            targetTeamId,
            targetUserId,
            isEnabled);

        entity.GetType().GetProperty(nameof(SlaEscalationRule.SlaPolicyId))!.SetValue(entity, updated.SlaPolicyId);
        entity.GetType().GetProperty(nameof(SlaEscalationRule.MetricType))!.SetValue(entity, updated.MetricType);
        entity.GetType().GetProperty(nameof(SlaEscalationRule.TriggerBeforeOrAfterMinutes))!.SetValue(entity, updated.TriggerBeforeOrAfterMinutes);
        entity.GetType().GetProperty(nameof(SlaEscalationRule.ActionType))!.SetValue(entity, updated.ActionType);
        entity.GetType().GetProperty(nameof(SlaEscalationRule.TargetTeamId))!.SetValue(entity, updated.TargetTeamId);
        entity.GetType().GetProperty(nameof(SlaEscalationRule.TargetUserId))!.SetValue(entity, updated.TargetUserId);
        entity.Toggle(isEnabled);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AttachPolicyToTicketAsync(Guid ticketId, Guid slaPolicyId, DateTime createdAtUtc, CancellationToken cancellationToken)
    {
        var existing = await dbContext.TicketSlaInstances.FirstOrDefaultAsync(x => x.TicketId == ticketId, cancellationToken);
        if (existing is not null)
            throw new ConflictAppException("A ticket SLA instance already exists for this ticket.");

        var policy = await dbContext.SlaPolicies.FirstOrDefaultAsync(x => x.Id == slaPolicyId, cancellationToken)
            ?? throw new NotFoundAppException("SLA policy was not found.");

        var instance = new TicketSlaInstance(
            ticketId,
            slaPolicyId,
            createdAtUtc.AddMinutes(policy.FirstResponseTargetMinutes),
            createdAtUtc.AddMinutes(policy.ResolutionTargetMinutes))
        {
            TenantId = tenantContext.GetRequiredTenantId()
        };

        await dbContext.TicketSlaInstances.AddAsync(instance, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFirstResponseAsync(Guid ticketId, DateTime respondedAtUtc, CancellationToken cancellationToken)
    {
        var instance = await dbContext.TicketSlaInstances.FirstOrDefaultAsync(x => x.TicketId == ticketId, cancellationToken)
            ?? throw new NotFoundAppException("Ticket SLA instance was not found.");

        instance.MarkFirstResponse(respondedAtUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkResolvedAsync(Guid ticketId, DateTime resolvedAtUtc, CancellationToken cancellationToken)
    {
        var instance = await dbContext.TicketSlaInstances.FirstOrDefaultAsync(x => x.TicketId == ticketId, cancellationToken)
            ?? throw new NotFoundAppException("Ticket SLA instance was not found.");

        instance.MarkResolved(resolvedAtUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RunDueEscalationsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var instances = await dbContext.TicketSlaInstances
            .Include(x => x.SlaPolicy)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var rules = await dbContext.SlaEscalationRules
            .Where(x => !x.IsDeleted && x.IsEnabled)
            .ToListAsync(cancellationToken);

        var created = 0;

        foreach (var instance in instances)
        {
            instance.Evaluate(utcNow);

            var policyRules = rules.Where(x => x.SlaPolicyId == instance.SlaPolicyId);
            foreach (var rule in policyRules)
            {
                var dueAt = rule.MetricType == SlaMetricType.FirstResponse
                    ? instance.FirstResponseDueAtUtc
                    : instance.ResolutionDueAtUtc;

                var triggerAt = dueAt.AddMinutes(rule.TriggerBeforeOrAfterMinutes);

                if (utcNow < triggerAt)
                    continue;

                var note = $"Escalation executed for ticket {instance.TicketId} using {rule.ActionType}.";
                var run = new TicketEscalationRun(instance.TicketId, instance.Id, rule.Id, rule.MetricType, note)
                {
                    TenantId = tenantContext.GetRequiredTenantId()
                };

                var alreadyExists = await dbContext.TicketEscalationRuns.AnyAsync(
                    x => x.TicketId == instance.TicketId &&
                         x.EscalationRuleId == rule.Id &&
                         x.MetricType == rule.MetricType,
                    cancellationToken);

                if (alreadyExists)
                    continue;

                await dbContext.TicketEscalationRuns.AddAsync(run, cancellationToken);
                created++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    private async Task ClearDefaultsAsync(int priority, CancellationToken cancellationToken)
    {
        var defaults = await dbContext.SlaPolicies.Where(x => x.IsDefault && x.Priority == priority).ToListAsync(cancellationToken);
        foreach (var item in defaults)
            item.RemoveDefault();
    }
}
