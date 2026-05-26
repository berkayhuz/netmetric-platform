// <copyright file="ChannelConversation.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Omnichannel.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.Omnichannel.Domain.Entities;

public sealed class ChannelConversation : AuditableEntity
{
    private ChannelConversation()
    {
    }

    public ChannelConversation(Guid accountId, string subject, string customerDisplayName, ConversationStatus status, DateTime lastMessageAtUtc)
    {
        AccountId = accountId;
        Subject = string.IsNullOrWhiteSpace(subject) ? throw new ArgumentException("Conversation subject is required.", nameof(subject)) : subject.Trim();
        CustomerDisplayName = string.IsNullOrWhiteSpace(customerDisplayName) ? throw new ArgumentException("Customer display name is required.", nameof(customerDisplayName)) : customerDisplayName.Trim();
        Status = status;
        LastMessageAtUtc = lastMessageAtUtc;
        ProviderKey = "mock";
        Priority = ConversationPriority.Normal;
        UnreadCount = 0;
    }

    public Guid AccountId { get; private set; }
    public string Subject { get; private set; } = null!;
    public string CustomerDisplayName { get; private set; } = null!;
    public string ExternalConversationId { get; private set; } = null!;
    public string ExternalParticipantId { get; private set; } = null!;
    public string ProviderKey { get; private set; } = null!;
    public Guid? LeadId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public string? AssignedUserDisplayName { get; private set; }
    public ConversationStatus Status { get; private set; }
    public ConversationPriority Priority { get; private set; }
    public DateTime LastMessageAtUtc { get; private set; }
    public int UnreadCount { get; private set; }
    public DateTime? LastReadAtUtc { get; private set; }

    public void BindExternal(string externalConversationId, string externalParticipantId, string providerKey)
    {
        ExternalConversationId = string.IsNullOrWhiteSpace(externalConversationId)
            ? throw new ArgumentException("External conversation id is required.", nameof(externalConversationId))
            : externalConversationId.Trim();
        ExternalParticipantId = string.IsNullOrWhiteSpace(externalParticipantId)
            ? throw new ArgumentException("External participant id is required.", nameof(externalParticipantId))
            : externalParticipantId.Trim();
        ProviderKey = string.IsNullOrWhiteSpace(providerKey)
            ? throw new ArgumentException("Provider key is required.", nameof(providerKey))
            : providerKey.Trim().ToLowerInvariant();
    }

    public void MarkActivity(DateTime occurredAtUtc) => LastMessageAtUtc = occurredAtUtc;

    public void LinkLead(Guid leadId) => LeadId = leadId;

    public void LinkCustomer(Guid customerId) => CustomerId = customerId;

    public void SetStatus(ConversationStatus status) => Status = status;

    public void SetPriority(ConversationPriority priority) => Priority = priority;

    public void Assign(Guid assignedUserId, string assignedUserDisplayName)
    {
        AssignedUserId = assignedUserId;
        AssignedUserDisplayName = string.IsNullOrWhiteSpace(assignedUserDisplayName)
            ? throw new ArgumentException("Assigned user display name is required.", nameof(assignedUserDisplayName))
            : assignedUserDisplayName.Trim();
    }

    public void Unassign()
    {
        AssignedUserId = null;
        AssignedUserDisplayName = null;
    }

    public void IncrementUnread() => UnreadCount++;

    public void MarkRead(DateTime readAtUtc)
    {
        UnreadCount = 0;
        LastReadAtUtc = readAtUtc;
    }
}
