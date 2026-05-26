// <copyright file="ConversationNote.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.Omnichannel.Domain.Entities;

public sealed class ConversationNote : AuditableEntity
{
    private ConversationNote()
    {
    }

    public ConversationNote(Guid conversationId, Guid authorUserId, string authorDisplayName, string noteText, DateTime createdAtUtc)
    {
        ConversationId = conversationId;
        AuthorUserId = authorUserId;
        AuthorDisplayName = string.IsNullOrWhiteSpace(authorDisplayName)
            ? throw new ArgumentException("Author display name is required.", nameof(authorDisplayName))
            : authorDisplayName.Trim();
        NoteText = string.IsNullOrWhiteSpace(noteText)
            ? throw new ArgumentException("Note text is required.", nameof(noteText))
            : noteText.Trim();
        CreatedAtUtc = createdAtUtc;
    }

    public Guid ConversationId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public string AuthorDisplayName { get; private set; } = null!;
    public string NoteText { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
}
