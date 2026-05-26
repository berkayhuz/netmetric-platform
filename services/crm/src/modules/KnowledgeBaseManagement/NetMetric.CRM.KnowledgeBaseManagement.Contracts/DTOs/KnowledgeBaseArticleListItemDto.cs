// <copyright file="KnowledgeBaseArticleListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;

public sealed record KnowledgeBaseArticleListItemDto(Guid Id, Guid CategoryId, string CategoryName, string Title, string Slug, string? Summary, string Status, bool IsPublic, DateTime? PublishedAt);
