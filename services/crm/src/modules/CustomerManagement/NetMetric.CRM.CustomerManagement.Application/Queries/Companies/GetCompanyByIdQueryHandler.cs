// <copyright file="GetCompanyByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Companies;

public sealed class GetCompanyByIdQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetCompanyByIdQuery, CompanyDetailDto?>
{
    public async Task<CompanyDetailDto?> Handle(
        GetCompanyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.CompaniesResource);
        var canSeeContactData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CompaniesResource, "contactData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeTaxData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CompaniesResource, "taxData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeFinancialData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CompaniesResource, "financialData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeNotes = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CompaniesResource, "notes", scope.Permissions)
            .Visibility == FieldVisibility.Visible;

        var company = await dbContext.Companies
            .AsNoTracking()
            .Include(x => x.Addresses)
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.CompanyId, cancellationToken);

        if (company is null)
            return null;

        return new CompanyDetailDto(
            company.Id,
            company.Name,
            canSeeTaxData ? company.TaxNumber : null,
            canSeeTaxData ? company.TaxOffice : null,
            company.Website,
            canSeeContactData ? company.Email : null,
            canSeeContactData ? company.Phone : null,
            company.Sector,
            company.EmployeeCountRange,
            canSeeFinancialData ? company.AnnualRevenue : null,
            company.Description,
            canSeeNotes ? company.Notes : null,
            company.CompanyType,
            company.OwnerUserId,
            company.ParentCompanyId,
            company.IsActive,
            company.Addresses
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.AddressType)
                .Select(x => new AddressDto(
                    x.Id,
                    x.AddressType,
                    x.Line1,
                    x.Line2,
                    x.District,
                    x.City,
                    x.State,
                    x.Country,
                    x.ZipCode,
                    x.IsDefault,
                    Convert.ToBase64String(x.RowVersion)))
                .ToList(),
            Convert.ToBase64String(company.RowVersion),
            company.LogoUrl);
    }
}
