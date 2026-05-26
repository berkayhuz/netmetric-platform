// <copyright file="ImportContactsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Imports;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportContacts;

public sealed class ImportContactsCommandHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<ImportContactsCommand, ImportExecutionResultDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ImportExecutionResultDto> Handle(ImportContactsCommand request, CancellationToken cancellationToken)
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

        var emails = document.Rows
            .Select(row => ImportValueParser.Get(row, "Email"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var companyIds = document.Rows
            .Select(row =>
            {
                var companyId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "CompanyId"));
                if (companyId.HasValue)
                    return companyId.Value;

                var companyName = ImportValueParser.Get(row, "CompanyName");
                return !string.IsNullOrWhiteSpace(companyName) && companiesByName.TryGetValue(companyName, out var resolvedCompanyId)
                    ? resolvedCompanyId
                    : Guid.Empty;
            })
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var existingContacts = await _dbContext.Set<Contact>()
            .Where(x => x.TenantId == tenantId
                && ((x.Email != null && emails.Contains(x.Email))
                    || (x.CompanyId.HasValue && companyIds.Contains(x.CompanyId.Value))))
            .ToListAsync(cancellationToken);

        var contactsByEmail = existingContacts
            .Where(x => !string.IsNullOrWhiteSpace(x.Email))
            .GroupBy(x => x.Email!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var contactsByNameCompany = existingContacts
            .GroupBy(x => BuildContactLookupKey(x.FirstName, x.LastName, x.CompanyId)!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var primaryCompanyIds = document.Rows
            .Where(row => ImportValueParser.ParseBool(ImportValueParser.Get(row, "IsPrimaryContact")) == true)
            .Select(row =>
            {
                var companyId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "CompanyId"));
                if (companyId.HasValue)
                    return companyId;

                var companyName = ImportValueParser.Get(row, "CompanyName");
                return !string.IsNullOrWhiteSpace(companyName) && companiesByName.TryGetValue(companyName, out var resolved)
                    ? resolved
                    : null;
            })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var primaryContactsByCompanyId = primaryCompanyIds.Count == 0
            ? new Dictionary<Guid, List<Contact>>()
            : await _dbContext.Set<Contact>()
                .Where(x => x.TenantId == tenantId && x.CompanyId.HasValue && primaryCompanyIds.Contains(x.CompanyId.Value) && x.IsPrimaryContact)
                .GroupBy(x => x.CompanyId!.Value)
                .ToDictionaryAsync(x => x.Key, x => x.ToList(), cancellationToken);

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
            var isPrimary = ImportValueParser.ParseBool(ImportValueParser.Get(row, "IsPrimaryContact")) ?? false;
            var isActive = ImportValueParser.ParseBool(ImportValueParser.Get(row, "IsActive")) ?? true;

            var companyId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "CompanyId"));
            var companyName = ImportValueParser.Get(row, "CompanyName");
            if (!companyId.HasValue && !string.IsNullOrWhiteSpace(companyName))
            {
                if (companiesByName.TryGetValue(companyName, out var resolvedCompanyId))
                    companyId = resolvedCompanyId;
            }

            var customerId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "CustomerId"));

            Contact? entity = null;

            if (!string.IsNullOrWhiteSpace(email))
                contactsByEmail.TryGetValue(email, out entity);

            if (entity is null)
                contactsByNameCompany.TryGetValue(BuildContactLookupKey(firstName, lastName, companyId) ?? string.Empty, out entity);

            if (isPrimary && companyId.HasValue && !request.DryRun)
            {
                primaryContactsByCompanyId.TryGetValue(companyId.Value, out var otherPrimaries);
                otherPrimaries ??= [];

                foreach (var item in otherPrimaries.Where(x => entity == null || x.Id != entity.Id))
                {
                    item.IsPrimaryContact = false;
                    item.UpdatedAt = DateTime.UtcNow;
                    item.UpdatedBy = actor;
                }
            }

            if (entity is null)
            {
                entity = new Contact
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
                    CustomerId = customerId,
                    IsPrimaryContact = isPrimary,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = actor,
                    UpdatedBy = actor
                };

                if (!isActive)
                    entity.Deactivate();

                if (!request.DryRun)
                    await _dbContext.Set<Contact>().AddAsync(entity, cancellationToken);

                if (!string.IsNullOrWhiteSpace(entity.Email))
                    contactsByEmail[entity.Email] = entity;

                contactsByNameCompany[BuildContactLookupKey(entity.FirstName, entity.LastName, entity.CompanyId)!] = entity;

                if (isPrimary && companyId.HasValue)
                    primaryContactsByCompanyId[companyId.Value] = [entity];

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
            entity.CustomerId = customerId;
            entity.IsPrimaryContact = isPrimary;
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
            "contact",
            request.DryRun,
            document.Rows.Count,
            created,
            updated,
            skipped,
            errors.Count,
            errors);
    }

    private static string? BuildContactLookupKey(
        string? firstName,
        string? lastName,
        Guid? companyId,
        string? companyName = null,
        IReadOnlyDictionary<string, Guid>? companiesByName = null)
    {
        if (!companyId.HasValue && !string.IsNullOrWhiteSpace(companyName) && companiesByName is not null && companiesByName.TryGetValue(companyName, out var resolvedCompanyId))
            companyId = resolvedCompanyId;

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || !companyId.HasValue)
            return null;

        return $"{firstName.Trim()}|{lastName.Trim()}|{companyId.Value}";
    }
}
