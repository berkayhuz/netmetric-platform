// <copyright file="BulkSoftDeleteContactsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Bulk;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Bulk.Commands.BulkSoftDeleteContacts;

public sealed class BulkSoftDeleteContactsCommandHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<BulkSoftDeleteContactsCommand, BulkOperationResultDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<BulkOperationResultDto> Handle(BulkSoftDeleteContactsCommand request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;
        var actor = _currentUserService.UserName ?? "SYSTEM";
        var now = DateTime.UtcNow;
        var ids = request.ContactIds.Where(x => x != Guid.Empty).Distinct().ToList();

        var entities = await _dbContext.Set<Contact>()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = now;
            entity.DeletedBy = actor;
            entity.UpdatedAt = now;
            entity.UpdatedBy = actor;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var foundIds = entities.Select(x => x.Id).ToHashSet();
        var missingIds = ids.Where(x => !foundIds.Contains(x)).ToList();

        return new BulkOperationResultDto
        {
            RequestedCount = ids.Count,
            AffectedCount = entities.Count,
            MissingIds = missingIds,
            Message = "Contact bulk soft delete completed."
        };
    }
}
