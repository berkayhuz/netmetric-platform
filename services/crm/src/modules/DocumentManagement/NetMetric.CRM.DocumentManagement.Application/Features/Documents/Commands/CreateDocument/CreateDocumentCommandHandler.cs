// <copyright file="CreateDocumentCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using NetMetric.CRM.DocumentManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentRecords;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Documents.Commands.CreateDocument;

public sealed class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Guid>
{
    private readonly IDocumentManagementDbContext _dbContext;

    public CreateDocumentCommandHandler(IDocumentManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var dataJson = JsonSerializer.Serialize(new
        {
            request.ContentType,
            request.SizeBytes
        });
        var entity = DocumentRecord.Create(request.Name, dataJson: dataJson);
        await _dbContext.DocumentRecords.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
