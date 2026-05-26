// <copyright file="GetKnowledgeBaseArticlesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Queries.Articles.GetKnowledgeBaseArticles;

public sealed record GetKnowledgeBaseArticlesQuery(string? Search, Guid? CategoryId, bool? PublishedOnly, int Page, int PageSize) : IRequest<PagedResult<KnowledgeBaseArticleListItemDto>>;
