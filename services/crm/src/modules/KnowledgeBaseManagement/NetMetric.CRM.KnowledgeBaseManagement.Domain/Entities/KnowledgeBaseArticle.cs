// <copyright file="KnowledgeBaseArticle.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.KnowledgeBaseManagement.Domain.Enums;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;

public sealed class KnowledgeBaseArticle : AuditableEntity
{
    private KnowledgeBaseArticle() { }

    public KnowledgeBaseArticle(Guid categoryId, string title, string? summary, string content, bool isPublic)
    {
        CategoryId = categoryId;
        Title = Guard.AgainstNullOrWhiteSpace(title);
        Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        Content = Guard.AgainstNullOrWhiteSpace(content);
        IsPublic = isPublic;
        Slug = BuildSlug(title);
        Status = KnowledgeBaseArticleStatus.Draft;
    }

    public Guid CategoryId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Summary { get; private set; }
    public string Content { get; private set; } = null!;
    public bool IsPublic { get; private set; }
    public KnowledgeBaseArticleStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    public void Update(Guid categoryId, string title, string? summary, string content, bool isPublic)
    {
        CategoryId = categoryId;
        Title = Guard.AgainstNullOrWhiteSpace(title);
        Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        Content = Guard.AgainstNullOrWhiteSpace(content);
        IsPublic = isPublic;
        Slug = BuildSlug(title);
    }

    public void Publish(DateTime publishedAtUtc)
    {
        if (Status == KnowledgeBaseArticleStatus.Published)
            return;

        Status = KnowledgeBaseArticleStatus.Published;
        PublishedAt = publishedAtUtc;
    }

    public void Archive()
    {
        Status = KnowledgeBaseArticleStatus.Archived;
    }

    private static string BuildSlug(string value)
        => value.Trim().ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal).Replace("--", "-", StringComparison.Ordinal);
}
