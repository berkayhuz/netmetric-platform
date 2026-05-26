// <copyright file="ChannelMessage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.Omnichannel.Domain.Entities;

public sealed class ChannelMessage : AuditableEntity
{
    private ChannelMessage()
    {
    }

    public ChannelMessage(Guid conversationId, string direction, string body, DateTime sentAtUtc, string externalMessageId)
    {
        ConversationId = conversationId;
        Direction = string.IsNullOrWhiteSpace(direction) ? throw new ArgumentException("Direction is required.", nameof(direction)) : direction.Trim();
        Body = string.IsNullOrWhiteSpace(body) ? string.Empty : body.Trim();
        SentAtUtc = sentAtUtc;
        ExternalMessageId = string.IsNullOrWhiteSpace(externalMessageId) ? throw new ArgumentException("External message id is required.", nameof(externalMessageId)) : externalMessageId.Trim();
        SenderDisplayName = "Unknown";
        Status = "sent";
    }

    public Guid ConversationId { get; private set; }
    public string Direction { get; private set; } = null!;
    public string Body { get; private set; } = string.Empty;
    public DateTime SentAtUtc { get; private set; }
    public string ExternalMessageId { get; private set; } = null!;
    public string SenderDisplayName { get; private set; } = null!;
    public string Status { get; private set; } = null!;

    public void SetSender(string senderDisplayName)
    {
        SenderDisplayName = string.IsNullOrWhiteSpace(senderDisplayName) ? "Unknown" : senderDisplayName.Trim();
    }

    public void SetStatus(string status)
    {
        Status = string.IsNullOrWhiteSpace(status) ? "sent" : status.Trim().ToLowerInvariant();
    }
}
