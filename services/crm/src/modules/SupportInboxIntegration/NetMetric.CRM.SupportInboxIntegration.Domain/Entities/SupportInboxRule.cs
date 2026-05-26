// <copyright file="SupportInboxRule.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

public sealed class SupportInboxRule : AuditableEntity
{
    private SupportInboxRule() { }

    public SupportInboxRule(Guid connectionId, string name, string? matchSender, string? matchSubjectContains, Guid? assignToQueueId, Guid? ticketCategoryId, Guid? slaPolicyId, bool autoCreateTicket)
    {
        ConnectionId = connectionId;
        Name = Guard.AgainstNullOrWhiteSpace(name);
        MatchSender = string.IsNullOrWhiteSpace(matchSender) ? null : matchSender.Trim();
        MatchSubjectContains = string.IsNullOrWhiteSpace(matchSubjectContains) ? null : matchSubjectContains.Trim();
        AssignToQueueId = assignToQueueId;
        TicketCategoryId = ticketCategoryId;
        SlaPolicyId = slaPolicyId;
        AutoCreateTicket = autoCreateTicket;
    }

    public Guid ConnectionId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? MatchSender { get; private set; }
    public string? MatchSubjectContains { get; private set; }
    public Guid? AssignToQueueId { get; private set; }
    public Guid? TicketCategoryId { get; private set; }
    public Guid? SlaPolicyId { get; private set; }
    public bool AutoCreateTicket { get; private set; }
    public void Update(string name, string? matchSender, string? matchSubjectContains, Guid? assignToQueueId, Guid? ticketCategoryId, Guid? slaPolicyId, bool autoCreateTicket, bool isActive)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        MatchSender = string.IsNullOrWhiteSpace(matchSender) ? null : matchSender.Trim();
        MatchSubjectContains = string.IsNullOrWhiteSpace(matchSubjectContains) ? null : matchSubjectContains.Trim();
        AssignToQueueId = assignToQueueId;
        TicketCategoryId = ticketCategoryId;
        SlaPolicyId = slaPolicyId;
        AutoCreateTicket = autoCreateTicket;
        SetActive(isActive);
    }
}
