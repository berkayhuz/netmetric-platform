// <copyright file="ArchiveKnowledgeBaseArticleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.ArchiveKnowledgeBaseArticle;

public sealed record ArchiveKnowledgeBaseArticleCommand(Guid ArticleId) : IRequest;
