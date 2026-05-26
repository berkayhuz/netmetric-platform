// <copyright file="ImportCustomersCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Imports;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCustomers;

public sealed class ImportCustomersCommandHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<ImportCustomersCommand, ImportExecutionResultDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ImportExecutionResultDto> Handle(ImportCustomersCommand request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();

        var tenantId = _currentUserService.TenantId;
        if (tenantId == Guid.Empty)
            throw new UnauthorizedAccessException("A tenant context is required.");

        if (request.TenantId != Guid.Empty && request.TenantId != tenantId)
            throw new UnauthorizedAccessException("The requested tenant does not match the authenticated tenant context.");

        var actor = _currentUserService.UserName ?? _currentUserService.Email ?? _currentUserService.UserId.ToString();
        var document = CsvImportParser.Parse(request.CsvContent, request.Separator);
        var errors = new List<ImportFailureDto>();
        var created = 0;
        var updated = 0;
        var skipped = 0;
        var companyNames = document.Rows
            .Select(row => ImportValueParser.Get(row, "CompanyName"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var companiesByName = await _dbContext.Set<Company>()
            .Where(x => x.TenantId == tenantId && companyNames.Contains(x.Name))
            .ToDictionaryAsync(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var identityNumbers = document.Rows
            .Select(row => ImportValueParser.Get(row, "IdentityNumber"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var emails = document.Rows
            .Select(row => ImportValueParser.Get(row, "Email"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var mobilePhones = document.Rows
            .Select(row => ImportValueParser.Get(row, "MobilePhone", "Phone"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingCustomers = await _dbContext.Set<Customer>()
            .Where(x => x.TenantId == tenantId
                && ((x.IdentityNumber != null && identityNumbers.Contains(x.IdentityNumber))
                    || (x.Email != null && emails.Contains(x.Email))
                    || (x.MobilePhone != null && mobilePhones.Contains(x.MobilePhone))))
            .ToListAsync(cancellationToken);

        var customersByIdentityNumber = existingCustomers
            .Where(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
            .GroupBy(x => x.IdentityNumber!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var customersByEmail = existingCustomers
            .Where(x => !string.IsNullOrWhiteSpace(x.Email))
            .GroupBy(x => x.Email!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var customersByNamePhone = existingCustomers
            .GroupBy(x => BuildCustomerLookupKey(x.FirstName, x.LastName, x.MobilePhone)!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < document.Rows.Count; index++)
        {
            var rowNumber = index + 2;
            var row = document.Rows[index];

            var firstName = ImportValueParser.Get(row, "FirstName");
            var lastName = ImportValueParser.Get(row, "LastName");
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                errors.Add(new ImportFailureDto(rowNumber, "FirstName and LastName are required."));
                continue;
            }

            var identityNumber = ImportValueParser.Get(row, "IdentityNumber");
            var email = ImportValueParser.Get(row, "Email");
            var mobilePhone = ImportValueParser.Get(row, "MobilePhone", "Phone");
            var workPhone = ImportValueParser.Get(row, "WorkPhone");
            var personalPhone = ImportValueParser.Get(row, "PersonalPhone");
            var title = ImportValueParser.Get(row, "Title");
            var department = ImportValueParser.Get(row, "Department");
            var jobTitle = ImportValueParser.Get(row, "JobTitle");
            var description = ImportValueParser.Get(row, "Description");
            var ownerUserId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "OwnerUserId"));
            var birthDate = ImportValueParser.ParseDate(ImportValueParser.Get(row, "BirthDate"));
            var gender = ImportValueParser.ParseGenderOrDefault(ImportValueParser.Get(row, "Gender"));
            var customerType = ImportValueParser.ParseEnum<CustomerType>(ImportValueParser.Get(row, "CustomerType")) ?? CustomerType.Individual;
            var isVip = ImportValueParser.ParseBool(ImportValueParser.Get(row, "IsVip")) ?? false;
            var isActive = ImportValueParser.ParseBool(ImportValueParser.Get(row, "IsActive")) ?? true;

            var companyId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "CompanyId"));
            var companyName = ImportValueParser.Get(row, "CompanyName");
            if (!companyId.HasValue && !string.IsNullOrWhiteSpace(companyName))
            {
                if (companiesByName.TryGetValue(companyName, out var resolvedCompanyId))
                    companyId = resolvedCompanyId;
            }

            Customer? entity = null;

            if (!string.IsNullOrWhiteSpace(identityNumber))
                customersByIdentityNumber.TryGetValue(identityNumber, out entity);

            if (entity is null && !string.IsNullOrWhiteSpace(email))
                customersByEmail.TryGetValue(email, out entity);

            if (entity is null)
                customersByNamePhone.TryGetValue(BuildCustomerLookupKey(firstName, lastName, mobilePhone) ?? string.Empty, out entity);

            if (entity is null)
            {
                entity = new Customer
                {
                    TenantId = tenantId,
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    Email = email,
                    MobilePhone = mobilePhone,
                    WorkPhone = workPhone,
                    PersonalPhone = personalPhone,
                    Title = title,
                    Department = department,
                    JobTitle = jobTitle,
                    Description = description,
                    OwnerUserId = ownerUserId,
                    BirthDate = birthDate,
                    Gender = gender,
                    CompanyId = companyId,
                    CustomerType = customerType,
                    IdentityNumber = identityNumber,
                    IsVip = isVip,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = actor,
                    UpdatedBy = actor
                };

                if (!isActive)
                    entity.Deactivate();

                if (!request.DryRun)
                    await _dbContext.Set<Customer>().AddAsync(entity, cancellationToken);

                if (!string.IsNullOrWhiteSpace(entity.IdentityNumber))
                    customersByIdentityNumber[entity.IdentityNumber] = entity;

                if (!string.IsNullOrWhiteSpace(entity.Email))
                    customersByEmail[entity.Email] = entity;

                customersByNamePhone[BuildCustomerLookupKey(entity.FirstName, entity.LastName, entity.MobilePhone)!] = entity;

                created++;
                continue;
            }

            if (!request.UpsertExisting)
            {
                skipped++;
                continue;
            }

            entity.FirstName = firstName.Trim();
            entity.LastName = lastName.Trim();
            entity.Email = email;
            entity.MobilePhone = mobilePhone;
            entity.WorkPhone = workPhone;
            entity.PersonalPhone = personalPhone;
            entity.Title = title;
            entity.Department = department;
            entity.JobTitle = jobTitle;
            entity.Description = description;
            entity.OwnerUserId = ownerUserId;
            entity.BirthDate = birthDate;
            entity.Gender = gender;
            entity.CompanyId = companyId;
            entity.CustomerType = customerType;
            entity.IdentityNumber = identityNumber;
            entity.IsVip = isVip;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = actor;

            if (isActive)
                entity.Activate();
            else
                entity.Deactivate();

            updated++;
        }

        if (!request.DryRun && (created > 0 || updated > 0))
            await _dbContext.SaveChangesAsync(cancellationToken);

        return new ImportExecutionResultDto(
            "customer",
            request.DryRun,
            document.Rows.Count,
            created,
            updated,
            skipped,
            errors.Count,
            errors);
    }

    private static string? BuildCustomerLookupKey(string? firstName, string? lastName, string? mobilePhone)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(mobilePhone))
            return null;

        return $"{firstName.Trim()}|{lastName.Trim()}|{mobilePhone.Trim()}";
    }
}
