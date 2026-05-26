// <copyright file="ListNotesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Notes;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Notes.Queries.ListNotes;

public sealed class ListNotesQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<ListNotesQuery, IReadOnlyList<NoteDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<IReadOnlyList<NoteDto>> Handle(ListNotesQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;
        var entityName = request.EntityName.Trim().ToLowerInvariant();

        var query = _dbContext.Set<Note>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .AsQueryable();

        query = entityName switch
        {
            EntityNames.Company => query.Where(x => x.CompanyId == request.EntityId),
            EntityNames.Contact => query.Where(x => x.ContactId == request.EntityId),
            EntityNames.Customer => query.Where(x => x.CustomerId == request.EntityId),
            _ => query.Where(_ => false)
        };

        return await query
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new NoteDto
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                IsPinned = x.IsPinned,
                EntityName = entityName,
                EntityId = request.EntityId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
