// <copyright file="UpdateKnowledgeBaseArticleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.UpdateKnowledgeBaseArticle;

public sealed record UpdateKnowledgeBaseArticleCommand(Guid ArticleId, Guid CategoryId, string Title, string? Summary, string Content, bool IsPublic) : IRequest<KnowledgeBaseArticleDetailDto>;
