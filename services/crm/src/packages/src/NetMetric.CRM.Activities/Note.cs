// <copyright file="Note.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Activities;

public class Note : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? TicketId { get; set; }
}
