// <copyright file="TicketUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.Contracts.Requests;

public sealed class TicketUpsertRequest
{
    public string Subject { get; set; } = null!;
    public string? Description { get; set; }
    public TicketType TicketType { get; set; } = TicketType.Support;
    public TicketChannelType Channel { get; set; } = TicketChannelType.Web;
    public PriorityType Priority { get; set; } = PriorityType.Medium;
    public Guid? AssignedUserId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? TicketCategoryId { get; set; }
    public Guid? SlaPolicyId { get; set; }
    public DateTime? FirstResponseDueAt { get; set; }
    public DateTime? ResolveDueAt { get; set; }
    public string? Notes { get; set; }
    public byte[]? RowVersion { get; set; }
}
