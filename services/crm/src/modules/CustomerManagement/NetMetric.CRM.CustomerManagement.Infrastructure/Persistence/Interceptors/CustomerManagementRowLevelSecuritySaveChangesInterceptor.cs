// <copyright file="CustomerManagementRowLevelSecuritySaveChangesInterceptor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Interceptors;

public sealed class CustomerManagementRowLevelSecuritySaveChangesInterceptor(
    ICurrentUserService currentUserService,
    IOptions<CustomerManagementSecurityOptions> options) : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly CustomerManagementSecurityOptions _options = options.Value;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyRowLevelSecurity(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyRowLevelSecurity(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyRowLevelSecurity(DbContext? dbContext)
    {
        if (dbContext is null || !_currentUserService.IsAuthenticated || _currentUserService.HasPermission(Permissions.RowSecurityBypass))
            return;

        var userId = _currentUserService.UserId;
        var entries = dbContext.ChangeTracker.Entries()
            .Where(x => x.Entity is Company or Contact or Customer)
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var allowed = entry.Entity switch
            {
                Company company => company.OwnerUserId == userId || (_options.AllowUnassignedWrite && company.OwnerUserId == null),
                Contact contact => contact.OwnerUserId == userId || (_options.AllowUnassignedWrite && contact.OwnerUserId == null),
                Customer customer => customer.OwnerUserId == userId || (_options.AllowUnassignedWrite && customer.OwnerUserId == null),
                _ => true
            };

            if (!allowed)
                throw new ForbiddenAppException("Row-level security denied the requested write operation.");
        }
    }
}
