// <copyright file="GlobalTrashIndexWriter.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class GlobalTrashIndexWriter(
    CustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IGlobalTrashIndexWriter
{
    private const int RetentionDays = 7;

    public async Task AddContactDeletionAsync(Contact contact, CancellationToken cancellationToken = default)
        => await AddDeletionAsync(
            contact.TenantId,
            CrmTrashEntityTypes.Contact,
            contact.Id,
            BuildDisplayName(contact),
            string.IsNullOrWhiteSpace(contact.Email) ? null : contact.Email,
            "contacts",
            $"/contacts/{contact.Id:D}",
            cancellationToken);

    public async Task AddCustomerDeletionAsync(Customer customer, CancellationToken cancellationToken = default)
        => await AddDeletionAsync(
            customer.TenantId,
            CrmTrashEntityTypes.Customer,
            customer.Id,
            BuildDisplayName(customer),
            string.IsNullOrWhiteSpace(customer.Email) ? null : customer.Email,
            "customers",
            $"/customers/{customer.Id:D}",
            cancellationToken);

    public async Task AddCompanyDeletionAsync(Company company, CancellationToken cancellationToken = default)
        => await AddDeletionAsync(
            company.TenantId,
            CrmTrashEntityTypes.Company,
            company.Id,
            BuildDisplayName(company),
            string.IsNullOrWhiteSpace(company.Website) ? company.Email : company.Website,
            "companies",
            $"/companies/{company.Id:D}",
            cancellationToken);

    private async Task AddDeletionAsync(
        Guid entityTenantId,
        string entityType,
        Guid entityId,
        string displayName,
        string? summary,
        string sourceModule,
        string originalRoute,
        CancellationToken cancellationToken)
    {
        var tenantId = entityTenantId != Guid.Empty ? entityTenantId : currentUserService.TenantId;
        if (tenantId == Guid.Empty)
        {
            return;
        }

        var hasPendingActive = dbContext.ChangeTracker.Entries<GlobalTrashItem>()
            .Any(x =>
                x.State != EntityState.Deleted
                && x.Entity.TenantId == tenantId
                && x.Entity.EntityType == entityType
                && x.Entity.EntityId == entityId
                && x.Entity.Status == CrmTrashStatuses.Active);

        if (hasPendingActive)
        {
            return;
        }

        var alreadyExists = await dbContext.Set<GlobalTrashItem>()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.EntityType == entityType
                     && x.EntityId == entityId
                     && x.Status == CrmTrashStatuses.Active,
                cancellationToken);

        if (alreadyExists)
        {
            return;
        }

        var deletedAtUtc = DateTime.UtcNow;
        Guid? userId = currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId;
        var deletedByDisplayName = !string.IsNullOrWhiteSpace(currentUserService.UserName)
            ? currentUserService.UserName
            : currentUserService.Email;

        await dbContext.Set<GlobalTrashItem>().AddAsync(
            new GlobalTrashItem
            {
                TenantId = tenantId,
                EntityType = entityType,
                EntityId = entityId,
                DisplayName = displayName,
                Summary = summary,
                SourceModule = sourceModule,
                OriginalRoute = originalRoute,
                DeletedAtUtc = deletedAtUtc,
                DeletedByUserId = userId,
                DeletedByDisplayName = deletedByDisplayName,
                ExpiresAtUtc = deletedAtUtc.AddDays(RetentionDays),
                Status = CrmTrashStatuses.Active,
                MetadataJson = null,
                AuditCorrelationId = Activity.Current?.TraceId.ToString()
            },
            cancellationToken);
    }

    private static string BuildDisplayName(Contact contact)
    {
        var fullName = contact.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            return contact.Email!;
        }

        return "Deleted contact";
    }

    private static string BuildDisplayName(Customer customer)
    {
        var fullName = customer.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        if (!string.IsNullOrWhiteSpace(customer.Email))
        {
            return customer.Email!;
        }

        return "Deleted customer";
    }

    private static string BuildDisplayName(Company company)
    {
        var name = company.Name.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        if (!string.IsNullOrWhiteSpace(company.Email))
        {
            return company.Email!;
        }

        return "Deleted company";
    }
}
