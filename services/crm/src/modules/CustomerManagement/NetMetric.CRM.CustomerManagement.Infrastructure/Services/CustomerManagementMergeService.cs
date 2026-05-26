// <copyright file="CustomerManagementMergeService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Services;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomFields;
using NetMetric.CRM.Documents;
using NetMetric.CRM.Tagging;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class CustomerManagementMergeService(ICustomerManagementDbContext dbContext) : ICustomerManagementMergeService
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task MergeCompaniesAsync(Guid targetCompanyId, Guid sourceCompanyId, CancellationToken cancellationToken = default)
    {
        if (targetCompanyId == sourceCompanyId)
            throw new ConflictAppException("Target and source company cannot be the same.");

        var target = await _dbContext.Set<Company>().FirstOrDefaultAsync(x => x.Id == targetCompanyId, cancellationToken)
            ?? throw new NotFoundAppException("Target company not found.");
        var source = await _dbContext.Set<Company>().FirstOrDefaultAsync(x => x.Id == sourceCompanyId, cancellationToken)
            ?? throw new NotFoundAppException("Source company not found.");

        target.Email ??= source.Email;
        target.Phone ??= source.Phone;
        target.Website ??= source.Website;
        target.TaxNumber ??= source.TaxNumber;
        target.TaxOffice ??= source.TaxOffice;
        target.Sector ??= source.Sector;
        target.Description ??= source.Description;
        target.OwnerUserId ??= source.OwnerUserId;

        await _dbContext.Set<Contact>()
            .Where(x => x.CompanyId == sourceCompanyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CompanyId, targetCompanyId), cancellationToken);

        await _dbContext.Set<Customer>()
            .Where(x => x.CompanyId == sourceCompanyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CompanyId, targetCompanyId), cancellationToken);

        await _dbContext.Set<Address>()
            .Where(x => x.CompanyId == sourceCompanyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CompanyId, targetCompanyId), cancellationToken);

        await _dbContext.Set<Note>()
            .Where(x => x.CompanyId == sourceCompanyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CompanyId, targetCompanyId), cancellationToken);

        await _dbContext.Set<Document>()
            .Where(x => x.CompanyId == sourceCompanyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CompanyId, targetCompanyId), cancellationToken);

        await _dbContext.Set<TagMap>()
            .Where(x => x.EntityType == EntityNames.Company && x.EntityId == sourceCompanyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EntityId, targetCompanyId), cancellationToken);

        await _dbContext.Set<CustomFieldValue>()
            .Where(x => x.EntityName == "company" && x.EntityId == sourceCompanyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EntityId, targetCompanyId), cancellationToken);

        _dbContext.Set<Company>().Remove(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MergeContactsAsync(Guid targetContactId, Guid sourceContactId, CancellationToken cancellationToken = default)
    {
        if (targetContactId == sourceContactId)
            throw new ConflictAppException("Target and source contact cannot be the same.");

        var target = await _dbContext.Set<Contact>().FirstOrDefaultAsync(x => x.Id == targetContactId, cancellationToken)
            ?? throw new NotFoundAppException("Target contact not found.");
        var source = await _dbContext.Set<Contact>().FirstOrDefaultAsync(x => x.Id == sourceContactId, cancellationToken)
            ?? throw new NotFoundAppException("Source contact not found.");

        target.Email ??= source.Email;
        target.MobilePhone ??= source.MobilePhone;
        target.WorkPhone ??= source.WorkPhone;
        target.PersonalPhone ??= source.PersonalPhone;
        target.Department ??= source.Department;
        target.JobTitle ??= source.JobTitle;
        target.CompanyId ??= source.CompanyId;
        target.CustomerId ??= source.CustomerId;
        target.IsPrimaryContact = target.IsPrimaryContact || source.IsPrimaryContact;

        await _dbContext.Set<Note>()
            .Where(x => x.ContactId == sourceContactId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ContactId, targetContactId), cancellationToken);

        await _dbContext.Set<Document>()
            .Where(x => x.ContactId == sourceContactId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ContactId, targetContactId), cancellationToken);

        await _dbContext.Set<TagMap>()
            .Where(x => x.EntityType == EntityNames.Contact && x.EntityId == sourceContactId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EntityId, targetContactId), cancellationToken);

        await _dbContext.Set<CustomFieldValue>()
            .Where(x => x.EntityName == "contact" && x.EntityId == sourceContactId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EntityId, targetContactId), cancellationToken);

        _dbContext.Set<Contact>().Remove(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MergeCustomersAsync(Guid targetCustomerId, Guid sourceCustomerId, CancellationToken cancellationToken = default)
    {
        if (targetCustomerId == sourceCustomerId)
            throw new ConflictAppException("Target and source customer cannot be the same.");

        var target = await _dbContext.Set<Customer>().FirstOrDefaultAsync(x => x.Id == targetCustomerId, cancellationToken)
            ?? throw new NotFoundAppException("Target customer not found.");
        var source = await _dbContext.Set<Customer>().FirstOrDefaultAsync(x => x.Id == sourceCustomerId, cancellationToken)
            ?? throw new NotFoundAppException("Source customer not found.");

        target.Email ??= source.Email;
        target.MobilePhone ??= source.MobilePhone;
        target.WorkPhone ??= source.WorkPhone;
        target.PersonalPhone ??= source.PersonalPhone;
        target.IdentityNumber ??= source.IdentityNumber;
        target.CompanyId ??= source.CompanyId;
        target.IsVip = target.IsVip || source.IsVip;

        await _dbContext.Set<Address>()
            .Where(x => x.CustomerId == sourceCustomerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CustomerId, targetCustomerId), cancellationToken);

        await _dbContext.Set<Contact>()
            .Where(x => x.CustomerId == sourceCustomerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CustomerId, targetCustomerId), cancellationToken);

        await _dbContext.Set<Note>()
            .Where(x => x.CustomerId == sourceCustomerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CustomerId, targetCustomerId), cancellationToken);

        await _dbContext.Set<Document>()
            .Where(x => x.CustomerId == sourceCustomerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CustomerId, targetCustomerId), cancellationToken);

        await _dbContext.Set<TagMap>()
            .Where(x => x.EntityType == EntityNames.Customer && x.EntityId == sourceCustomerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EntityId, targetCustomerId), cancellationToken);

        await _dbContext.Set<CustomFieldValue>()
            .Where(x => x.EntityName == "customer" && x.EntityId == sourceCustomerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EntityId, targetCustomerId), cancellationToken);

        _dbContext.Set<Customer>().Remove(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
