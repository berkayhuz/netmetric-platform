// <copyright file="TicketComment.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Support;

public class TicketComment : AuditableEntity
{
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public string Body { get; set; } = string.Empty;
    public string Comment
    {
        get => Body;
        set => Body = value;
    }
    public Guid? AuthorUserId { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
