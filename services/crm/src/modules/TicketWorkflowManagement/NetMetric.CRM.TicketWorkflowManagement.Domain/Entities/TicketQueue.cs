// <copyright file="TicketQueue.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.TicketWorkflowManagement.Domain.Enums;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

public sealed class TicketQueue : AuditableEntity
{
    private TicketQueue() { }

    public TicketQueue(string code, string name, TicketQueueAssignmentStrategy assignmentStrategy)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        AssignmentStrategy = assignmentStrategy;
    }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public TicketQueueAssignmentStrategy AssignmentStrategy { get; private set; }
    public bool IsDefault { get; private set; }

    public ICollection<TicketQueueMembership> Memberships { get; private set; } = new List<TicketQueueMembership>();

    public void Update(string name, string? description, TicketQueueAssignmentStrategy assignmentStrategy, bool isDefault)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        AssignmentStrategy = assignmentStrategy;
        IsDefault = isDefault;
    }
}
