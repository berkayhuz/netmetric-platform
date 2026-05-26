// <copyright file="ListDocumentsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.DocumentManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DocumentManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Documents.Queries.ListDocuments;

public sealed class ListDocumentsQueryHandler(
    IDocumentManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<ListDocumentsQuery, PagedResult<DocumentMetadataDto>>
{
    public async Task<PagedResult<DocumentMetadataDto>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.DocumentsResource);
        var canSeePreview = fieldAuthorizationService.Decide(scope.Resource, "preview", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var page = PageRequest.Normalize(request.Page, request.PageSize);

        var query = dbContext.DocumentRecords
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.EntityType == null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Name, search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var documents = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenBy(x => x.Name)
            .Skip(page.Skip)
            .Take(page.Size)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.DataJson
            })
            .ToListAsync(cancellationToken);

        var documentIds = documents.Select(x => x.Id).ToArray();

        var versionCounts = await dbContext.DocumentRecords
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId &&
                        x.RelatedEntityId.HasValue &&
                        documentIds.Contains(x.RelatedEntityId.Value) &&
                        x.EntityType == "document-version")
            .GroupBy(x => x.RelatedEntityId!.Value)
            .Select(group => new { DocumentId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.DocumentId, x => x.Count, cancellationToken);

        var items = documents
            .Select(x => new DocumentMetadataDto
            {
                DocumentId = x.Id,
                Name = x.Name,
                ContentType = DocumentMetadataJson.ReadString(x.DataJson, "ContentType") ?? "application/octet-stream",
                VersionCount = versionCounts.GetValueOrDefault(x.Id),
                PreviewUrl = canSeePreview ? DocumentMetadataJson.ReadString(x.DataJson, "PreviewUrl") : null
            })
            .ToList();

        return PagedResult<DocumentMetadataDto>.Create(items, totalCount, page);
    }
}
