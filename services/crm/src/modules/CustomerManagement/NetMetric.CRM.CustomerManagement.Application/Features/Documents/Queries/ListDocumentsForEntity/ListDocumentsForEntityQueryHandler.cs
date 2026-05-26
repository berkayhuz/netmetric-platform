// <copyright file="ListDocumentsForEntityQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Documents;
using NetMetric.CRM.Documents;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Documents.Queries.ListDocumentsForEntity;

public sealed class ListDocumentsForEntityQueryHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<ListDocumentsForEntityQuery, IReadOnlyList<DocumentReferenceDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<DocumentReferenceDto>> Handle(ListDocumentsForEntityQuery request, CancellationToken cancellationToken)
    {
        var entityName = request.EntityName.Trim().ToLowerInvariant();
        var query = _dbContext.Set<Document>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        query = entityName switch
        {
            EntityNames.Company => query.Where(x => x.CompanyId == request.EntityId),
            EntityNames.Contact => query.Where(x => x.ContactId == request.EntityId),
            EntityNames.Customer => query.Where(x => x.CustomerId == request.EntityId),
            _ => query.Where(_ => false)
        };

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DocumentReferenceDto
            {
                Id = x.Id,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType ?? "application/octet-stream",
                FileExtension = x.FileExtension,
                PathOrUrl = x.PathOrUrl,
                FileSize = x.FileSize,
                IsPrivate = x.IsPrivate,
                EntityName = entityName,
                EntityId = request.EntityId,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
