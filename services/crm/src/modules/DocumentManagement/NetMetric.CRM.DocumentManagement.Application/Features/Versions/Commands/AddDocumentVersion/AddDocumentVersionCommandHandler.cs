// <copyright file="AddDocumentVersionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DocumentManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentRecords;
using NetMetric.Exceptions;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Versions.Commands.AddDocumentVersion;

public sealed class AddDocumentVersionCommandHandler : IRequestHandler<AddDocumentVersionCommand, Guid>
{
    private readonly IDocumentManagementDbContext _dbContext;

    public AddDocumentVersionCommandHandler(IDocumentManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(AddDocumentVersionCommand request, CancellationToken cancellationToken)
    {
        var documentExists = await _dbContext.DocumentRecords
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.DocumentId && x.EntityType == null, cancellationToken);

        if (!documentExists)
        {
            throw new NotFoundAppException("Document not found.");
        }

        var dataJson = JsonSerializer.Serialize(new
        {
            request.DocumentId,
            request.FileName,
            request.StorageKey
        });
        var entity = DocumentRecord.Create(request.FileName, "document-version", request.DocumentId, dataJson);
        await _dbContext.DocumentRecords.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
