// <copyright file="CustomerManagementFieldSecuritySaveChangesInterceptor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Interceptors;

public sealed class CustomerManagementFieldSecuritySaveChangesInterceptor(
    ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyFieldSecurity(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyFieldSecurity(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyFieldSecurity(DbContext? dbContext)
    {
        if (dbContext is null || !_currentUserService.IsAuthenticated)
            return;

        if (_currentUserService.HasPermission(Permissions.FieldSecurityBypass) ||
            _currentUserService.HasPermission(Permissions.SensitiveDataManage))
        {
            return;
        }

        var protectedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Company.Email),
            nameof(Company.Phone),
            nameof(Company.TaxNumber),
            nameof(Contact.Email),
            nameof(Contact.MobilePhone),
            nameof(Contact.WorkPhone),
            nameof(Contact.PersonalPhone),
            nameof(Customer.Email),
            nameof(Customer.MobilePhone),
            nameof(Customer.WorkPhone),
            nameof(Customer.IdentityNumber)
        };

        var protectedEntries = dbContext.ChangeTracker.Entries()
            .Where(x => x.Entity is Company or Contact or Customer)
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in protectedEntries)
        {
            var modifiedSensitiveField = entry.Properties.Any(x =>
                protectedProperties.Contains(x.Metadata.Name) &&
                (entry.State == EntityState.Added || x.IsModified) &&
                x.CurrentValue is not null);

            if (modifiedSensitiveField)
                throw new ForbiddenAppException("Sensitive fields require elevated field-level security permission.");
        }
    }
}
