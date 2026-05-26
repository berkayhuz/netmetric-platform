// <copyright file="GetDocumentMetadataQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.DocumentManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DocumentManagement.Contracts.DTOs;
using NetMetric.Exceptions;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Documents.Queries.GetDocumentMetadata;

public sealed class GetDocumentMetadataQueryHandler(
    IDocumentManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetDocumentMetadataQuery, DocumentMetadataDto>
{
    public async Task<DocumentMetadataDto> Handle(GetDocumentMetadataQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.DocumentsResource);
        var canSeePreview = fieldAuthorizationService.Decide(scope.Resource, "preview", scope.Permissions).Visibility >= FieldVisibility.Visible;

        var document = await dbContext.DocumentRecords
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.Id == request.DocumentId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.DataJson
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new NotFoundException("Document", request.DocumentId);
        }

        var versionCount = await dbContext.DocumentRecords
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == scope.TenantId &&
                     x.RelatedEntityId == request.DocumentId &&
                     x.EntityType == "document-version",
                cancellationToken);

        return new DocumentMetadataDto
        {
            DocumentId = document.Id,
            Name = document.Name,
            ContentType = DocumentMetadataJson.ReadString(document.DataJson, "ContentType") ?? "application/octet-stream",
            VersionCount = versionCount,
            PreviewUrl = canSeePreview ? DocumentMetadataJson.ReadString(document.DataJson, "PreviewUrl") : null
        };
    }
}
