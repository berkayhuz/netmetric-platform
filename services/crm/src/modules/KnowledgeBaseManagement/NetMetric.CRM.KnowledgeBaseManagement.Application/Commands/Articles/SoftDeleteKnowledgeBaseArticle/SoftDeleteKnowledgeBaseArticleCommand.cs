// <copyright file="SoftDeleteKnowledgeBaseArticleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.SoftDeleteKnowledgeBaseArticle;

public sealed record SoftDeleteKnowledgeBaseArticleCommand(Guid ArticleId) : IRequest;
