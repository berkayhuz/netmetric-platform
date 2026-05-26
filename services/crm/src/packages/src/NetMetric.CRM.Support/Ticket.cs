// <copyright file="Ticket.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Support;

public class Ticket : AuditableEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketStatusType Status { get; set; }
    public PriorityType Priority { get; set; }
    public TicketType TicketType { get; set; }
    public TicketChannelType Channel { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? TicketCategoryId { get; set; }
    public TicketCategory? TicketCategory { get; set; }
    public Guid? SlaPolicyId { get; set; }
    public SlaPolicy? SlaPolicy { get; set; }
    public DateTime? FirstResponseDueAt { get; set; }
    public DateTime? ResolveDueAt { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Notes { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = [];

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
