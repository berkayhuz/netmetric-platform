// <copyright file="ImportCompaniesCommandHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCompanies;

public sealed class ImportCompaniesCommandHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<ImportCompaniesCommand, ImportExecutionResultDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ImportExecutionResultDto> Handle(ImportCompaniesCommand request, CancellationToken cancellationToken)
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
        var companyRows = document.Rows
            .Select(row => new
            {
                TaxNumber = ImportValueParser.Get(row, "TaxNumber", "TaxNo"),
                Name = ImportValueParser.Get(row, "Name", "CompanyName")
            })
            .ToList();

        var taxNumbers = companyRows
            .Select(x => x.TaxNumber)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var names = companyRows
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingCompanies = await _dbContext.Set<Company>()
            .Where(x => x.TenantId == tenantId
                && ((x.TaxNumber != null && taxNumbers.Contains(x.TaxNumber)) || names.Contains(x.Name)))
            .ToListAsync(cancellationToken);

        var companiesByTaxNumber = existingCompanies
            .Where(x => !string.IsNullOrWhiteSpace(x.TaxNumber))
            .GroupBy(x => x.TaxNumber!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var companiesByName = existingCompanies
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < document.Rows.Count; index++)
        {
            var rowNumber = index + 2;
            var row = document.Rows[index];

            var name = ImportValueParser.Get(row, "Name", "CompanyName");
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportFailureDto(rowNumber, "Name is required."));
                continue;
            }

            var taxNumber = ImportValueParser.Get(row, "TaxNumber", "TaxNo");
            var email = ImportValueParser.Get(row, "Email");
            var phone = ImportValueParser.Get(row, "Phone", "MobilePhone", "WorkPhone");
            var website = ImportValueParser.Get(row, "Website");
            var sector = ImportValueParser.Get(row, "Sector");
            var taxOffice = ImportValueParser.Get(row, "TaxOffice");
            var employeeCountRange = ImportValueParser.Get(row, "EmployeeCountRange");
            var description = ImportValueParser.Get(row, "Description");
            var annualRevenue = ImportValueParser.ParseDecimal(ImportValueParser.Get(row, "AnnualRevenue"));
            var companyType = ImportValueParser.ParseEnum<CompanyType>(ImportValueParser.Get(row, "CompanyType")) ?? CompanyType.Prospect;
            var ownerUserId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "OwnerUserId"));
            var parentCompanyId = ImportValueParser.ParseGuid(ImportValueParser.Get(row, "ParentCompanyId"));
            var isActive = ImportValueParser.ParseBool(ImportValueParser.Get(row, "IsActive")) ?? true;

            Company? entity = null;

            if (!string.IsNullOrWhiteSpace(taxNumber))
                companiesByTaxNumber.TryGetValue(taxNumber, out entity);

            if (entity is null)
                companiesByName.TryGetValue(name, out entity);

            if (entity is null)
            {
                entity = new Company
                {
                    TenantId = tenantId,
                    Name = name.Trim(),
                    TaxNumber = taxNumber,
                    TaxOffice = taxOffice,
                    Website = website,
                    Email = email,
                    Phone = phone,
                    Sector = sector,
                    EmployeeCountRange = employeeCountRange,
                    AnnualRevenue = annualRevenue,
                    Description = description,
                    CompanyType = companyType,
                    OwnerUserId = ownerUserId,
                    ParentCompanyId = parentCompanyId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = actor,
                    UpdatedBy = actor
                };

                if (!isActive)
                    entity.Deactivate();

                if (!request.DryRun)
                    await _dbContext.Set<Company>().AddAsync(entity, cancellationToken);

                if (!string.IsNullOrWhiteSpace(entity.TaxNumber))
                    companiesByTaxNumber[entity.TaxNumber] = entity;

                companiesByName[entity.Name] = entity;

                created++;
                continue;
            }

            if (!request.UpsertExisting)
            {
                skipped++;
                continue;
            }

            entity.Name = name.Trim();
            entity.TaxNumber = taxNumber;
            entity.TaxOffice = taxOffice;
            entity.Website = website;
            entity.Email = email;
            entity.Phone = phone;
            entity.Sector = sector;
            entity.EmployeeCountRange = employeeCountRange;
            entity.AnnualRevenue = annualRevenue;
            entity.Description = description;
            entity.CompanyType = companyType;
            entity.OwnerUserId = ownerUserId;
            entity.ParentCompanyId = parentCompanyId;
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
            "company",
            request.DryRun,
            document.Rows.Count,
            created,
            updated,
            skipped,
            errors.Count,
            errors);
    }
}
