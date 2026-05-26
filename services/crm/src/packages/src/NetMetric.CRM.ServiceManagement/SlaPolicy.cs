// <copyright file="SlaPolicy.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Guards;

namespace NetMetric.CRM.ServiceManagement;

public class SlaPolicy : AuditableEntity
{
    private SlaPolicy() { }

    public SlaPolicy(
        string name,
        Guid? ticketCategoryId,
        int priority,
        int firstResponseTargetMinutes,
        int resolutionTargetMinutes)
    {
        Rename(name);
        SetPriority(priority);
        SetTargets(firstResponseTargetMinutes, resolutionTargetMinutes);
        TicketCategoryId = ticketCategoryId;
        IsDefault = false;
    }

    public string Name { get; private set; } = null!;
    public Guid? TicketCategoryId { get; private set; }
    public int Priority { get; private set; }
    public int FirstResponseTargetMinutes { get; private set; }
    public int ResolutionTargetMinutes { get; private set; }
    public bool IsDefault { get; private set; }

    public void Rename(string name) => Name = Guard.AgainstNullOrWhiteSpace(name);

    public void RebindCategory(Guid? ticketCategoryId) => TicketCategoryId = ticketCategoryId;

    public void SetPriority(int priority)
    {
        if (priority < 1 || priority > 5)
            throw new InvalidOperationException("Priority must be between 1 and 5.");

        Priority = priority;
    }

    public void SetTargets(int firstResponseTargetMinutes, int resolutionTargetMinutes)
    {
        if (firstResponseTargetMinutes <= 0)
            throw new InvalidOperationException("First response target must be greater than zero.");

        if (resolutionTargetMinutes <= 0)
            throw new InvalidOperationException("Resolution target must be greater than zero.");

        if (resolutionTargetMinutes < firstResponseTargetMinutes)
            throw new InvalidOperationException("Resolution target cannot be shorter than first response target.");

        FirstResponseTargetMinutes = firstResponseTargetMinutes;
        ResolutionTargetMinutes = resolutionTargetMinutes;
    }

    public void MarkAsDefault() => IsDefault = true;
    public void RemoveDefault() => IsDefault = false;
}
