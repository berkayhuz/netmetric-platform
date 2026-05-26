// <copyright file="KnowledgeBaseArticleUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.KnowledgeBaseManagement.Contracts.Requests;

public sealed class KnowledgeBaseArticleUpsertRequest
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string Content { get; set; } = null!;
    public bool IsPublic { get; set; }
}
