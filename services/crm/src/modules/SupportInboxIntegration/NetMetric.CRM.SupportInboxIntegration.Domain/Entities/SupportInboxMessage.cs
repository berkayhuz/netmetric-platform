// <copyright file="SupportInboxMessage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.SupportInboxIntegration.Domain.Enums;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

public sealed class SupportInboxMessage : AuditableEntity
{
    private SupportInboxMessage() { }

    public SupportInboxMessage(Guid connectionId, string externalMessageId, string fromAddress, string subject, DateTime receivedAtUtc, string? normalizedText)
    {
        ConnectionId = connectionId;
        ExternalMessageId = Guard.AgainstNullOrWhiteSpace(externalMessageId);
        FromAddress = Guard.AgainstNullOrWhiteSpace(fromAddress);
        Subject = Guard.AgainstNullOrWhiteSpace(subject);
        ReceivedAtUtc = receivedAtUtc;
        NormalizedText = string.IsNullOrWhiteSpace(normalizedText) ? null : normalizedText.Trim();
        ProcessingStatus = SupportInboxMessageProcessingStatus.Received;
    }

    public Guid ConnectionId { get; private set; }
    public Guid? SyncRunId { get; private set; }
    public Guid? TicketId { get; private set; }
    public string ExternalMessageId { get; private set; } = null!;
    public string FromAddress { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string? NormalizedText { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public SupportInboxMessageProcessingStatus ProcessingStatus { get; private set; }
    public string? ProcessingError { get; private set; }

    public void AttachToRun(Guid syncRunId) => SyncRunId = syncRunId;
    public void MarkMatched(Guid? ticketId)
    {
        TicketId = ticketId;
        ProcessingStatus = ticketId.HasValue ? SupportInboxMessageProcessingStatus.TicketCreated : SupportInboxMessageProcessingStatus.Matched;
        ProcessingError = null;
    }

    public void MarkIgnored()
    {
        ProcessingStatus = SupportInboxMessageProcessingStatus.Ignored;
        ProcessingError = null;
    }

    public void MarkFailed(string error)
    {
        ProcessingStatus = SupportInboxMessageProcessingStatus.Failed;
        ProcessingError = Guard.AgainstNullOrWhiteSpace(error);
    }
}
