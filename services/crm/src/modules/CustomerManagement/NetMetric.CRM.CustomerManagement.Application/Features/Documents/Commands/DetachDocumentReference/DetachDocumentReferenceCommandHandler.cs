// <copyright file="DetachDocumentReferenceCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Documents;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Documents.Commands.DetachDocumentReference;

public sealed class DetachDocumentReferenceCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<DetachDocumentReferenceCommand, Unit>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Unit> Handle(DetachDocumentReferenceCommand request, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Set<Document>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.DocumentId, cancellationToken)
            ?? throw new NotFoundAppException("Document not found.");

        _dbContext.Set<Document>().Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
