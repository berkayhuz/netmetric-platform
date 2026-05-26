// <copyright file="IDocumentManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentAttachments;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentPreviewAssets;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentRecords;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentReviews;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentStorageProviders;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentVersions;
using NetMetric.Repository;

namespace NetMetric.CRM.DocumentManagement.Application.Abstractions.Persistence;

public interface IDocumentManagementDbContext : IUnitOfWork
{
    DbSet<DocumentRecord> DocumentRecords { get; }
    DbSet<DocumentAttachment> DocumentAttachments { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }
    DbSet<DocumentPreviewAsset> DocumentPreviewAssets { get; }
    DbSet<DocumentReview> DocumentReviews { get; }
    DbSet<DocumentStorageProvider> DocumentStorageProviders { get; }
}
