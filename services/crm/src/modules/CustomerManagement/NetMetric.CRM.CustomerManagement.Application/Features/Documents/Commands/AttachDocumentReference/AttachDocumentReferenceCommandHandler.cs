// <copyright file="AttachDocumentReferenceCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Documents;
using NetMetric.CRM.Documents;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Documents.Commands.AttachDocumentReference;

public sealed class AttachDocumentReferenceCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<AttachDocumentReferenceCommand, DocumentReferenceDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<DocumentReferenceDto> Handle(AttachDocumentReferenceCommand request, CancellationToken cancellationToken)
    {
        var document = new Document
        {
            FileName = request.FileName.Trim(),
            OriginalFileName = request.OriginalFileName.Trim(),
            FileExtension = request.FileExtension.Trim(),
            ContentType = request.ContentType.Trim(),
            PathOrUrl = request.PathOrUrl.Trim(),
            FileSize = request.FileSize,
            IsPrivate = request.IsPrivate
        };

        CustomerManagementEntityReferenceHelper.ApplyReference(document, request.EntityName, request.EntityId);

        await _dbContext.Set<Document>().AddAsync(document, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DocumentReferenceDto
        {
            Id = document.Id,
            FileName = document.FileName,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileExtension = document.FileExtension,
            PathOrUrl = document.PathOrUrl,
            FileSize = document.FileSize,
            IsPrivate = document.IsPrivate,
            EntityName = request.EntityName.ToLowerInvariant(),
            EntityId = request.EntityId,
            CreatedAt = document.CreatedAt
        };
    }
}
