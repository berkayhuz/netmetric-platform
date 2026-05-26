// <copyright file="IKnowledgeBaseManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;

public interface IKnowledgeBaseManagementDbContext
{
    DbSet<KnowledgeBaseCategory> Categories { get; }
    DbSet<KnowledgeBaseArticle> Articles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
