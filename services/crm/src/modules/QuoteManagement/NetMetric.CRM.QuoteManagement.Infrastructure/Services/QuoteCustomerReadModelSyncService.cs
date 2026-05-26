// <copyright file="QuoteCustomerReadModelSyncService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;
using NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Services;

public sealed class QuoteCustomerReadModelSyncService(
    QuoteManagementDbContext quoteDbContext,
    ICustomerManagementDbContext customerManagementDbContext,
    ICurrentUserService currentUserService) : IQuoteCustomerReadModelSyncService
{
    public async Task<QuoteCustomerReadModelSyncResult> SyncAsync(Guid customerId, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var sourceCustomer = await customerManagementDbContext.Customers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == customerId && !x.IsDeleted)
            .Select(x => new CustomerProjection(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Title,
                x.Email,
                x.MobilePhone,
                x.WorkPhone,
                x.PersonalPhone,
                x.BirthDate,
                x.Gender,
                x.Department,
                x.JobTitle,
                x.Description,
                x.Notes,
                x.OwnerUserId,
                x.CustomerType,
                x.IdentityNumber,
                x.IsVip,
                x.CompanyId))
            .SingleOrDefaultAsync(cancellationToken);

        if (sourceCustomer is null)
        {
            throw new ValidationAppException(
                "Selected customer is not available.",
                new Dictionary<string, string[]>
                {
                    ["customerId"] = ["Selected customer is not available."]
                });
        }

        var localCustomer = await quoteDbContext.Customers
            .FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);

        var utcNow = DateTime.UtcNow;
        var actor = currentUserService.UserName;

        if (localCustomer is null)
        {
            localCustomer = new CRM.Core.Customer
            {
                TenantId = tenantId,
                FirstName = sourceCustomer.FirstName,
                LastName = sourceCustomer.LastName,
                Title = sourceCustomer.Title,
                Email = sourceCustomer.Email,
                MobilePhone = sourceCustomer.MobilePhone,
                WorkPhone = sourceCustomer.WorkPhone,
                PersonalPhone = sourceCustomer.PersonalPhone,
                BirthDate = sourceCustomer.BirthDate,
                Gender = sourceCustomer.Gender,
                Department = sourceCustomer.Department,
                JobTitle = sourceCustomer.JobTitle,
                Description = sourceCustomer.Description,
                OwnerUserId = sourceCustomer.OwnerUserId,
                CustomerType = sourceCustomer.CustomerType,
                IdentityNumber = sourceCustomer.IdentityNumber,
                IsVip = sourceCustomer.IsVip,
                CompanyId = sourceCustomer.CompanyId,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                CreatedBy = actor,
                UpdatedBy = actor
            };
            localCustomer.SetNotes(sourceCustomer.Notes);

            var entry = quoteDbContext.Customers.Add(localCustomer);
            entry.Property(x => x.Id).CurrentValue = sourceCustomer.Id;
        }
        else
        {
            localCustomer.TenantId = tenantId;
            localCustomer.FirstName = sourceCustomer.FirstName;
            localCustomer.LastName = sourceCustomer.LastName;
            localCustomer.Title = sourceCustomer.Title;
            localCustomer.Email = sourceCustomer.Email;
            localCustomer.MobilePhone = sourceCustomer.MobilePhone;
            localCustomer.WorkPhone = sourceCustomer.WorkPhone;
            localCustomer.PersonalPhone = sourceCustomer.PersonalPhone;
            localCustomer.BirthDate = sourceCustomer.BirthDate;
            localCustomer.Gender = sourceCustomer.Gender;
            localCustomer.Department = sourceCustomer.Department;
            localCustomer.JobTitle = sourceCustomer.JobTitle;
            localCustomer.Description = sourceCustomer.Description;
            localCustomer.OwnerUserId = sourceCustomer.OwnerUserId;
            localCustomer.CustomerType = sourceCustomer.CustomerType;
            localCustomer.IdentityNumber = sourceCustomer.IdentityNumber;
            localCustomer.IsVip = sourceCustomer.IsVip;
            localCustomer.CompanyId = sourceCustomer.CompanyId;
            localCustomer.SetNotes(sourceCustomer.Notes);
            localCustomer.UpdatedAt = utcNow;
            localCustomer.UpdatedBy = actor;
        }

        return new QuoteCustomerReadModelSyncResult(sourceCustomer.Id);
    }

    private sealed record CustomerProjection(
        Guid Id,
        string FirstName,
        string LastName,
        string? Title,
        string? Email,
        string? MobilePhone,
        string? WorkPhone,
        string? PersonalPhone,
        DateTime? BirthDate,
        GenderType Gender,
        string? Department,
        string? JobTitle,
        string? Description,
        string? Notes,
        Guid? OwnerUserId,
        CustomerType CustomerType,
        string? IdentityNumber,
        bool IsVip,
        Guid? CompanyId);
}
