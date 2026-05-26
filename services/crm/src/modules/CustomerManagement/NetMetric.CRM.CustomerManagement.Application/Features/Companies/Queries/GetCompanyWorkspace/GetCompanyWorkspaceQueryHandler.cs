// <copyright file="GetCompanyWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Companies;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Companies.Queries.GetCompanyWorkspace;

public sealed class GetCompanyWorkspaceQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService) : IRequestHandler<GetCompanyWorkspaceQuery, CompanyWorkspaceDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<CompanyWorkspaceDto> Handle(GetCompanyWorkspaceQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;

        var company = await _securityService.ApplyCompanyReadScope(_dbContext.Set<Company>())
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == request.CompanyId)
            .Select(x => new CompanyWorkspaceDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                Website = x.Website,
                TaxNumber = x.TaxNumber,
                TaxOffice = x.TaxOffice,
                Sector = x.Sector,
                Description = x.Description,
                OwnerUserId = x.OwnerUserId,
                ParentCompanyId = x.ParentCompanyId,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null)
            throw new NotFoundAppException("Company not found.");

        var contactCountTask = _dbContext.Set<Contact>()
            .AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && !c.IsDeleted && c.CompanyId == company.Id, cancellationToken);
        var customerCountTask = _dbContext.Set<Customer>()
            .AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && !c.IsDeleted && c.CompanyId == company.Id, cancellationToken);
        var addressCountTask = _dbContext.Set<Address>()
            .AsNoTracking()
            .CountAsync(a => a.TenantId == tenantId && !a.IsDeleted && a.CompanyId == company.Id, cancellationToken);
        var childCompanyCountTask = _dbContext.Set<Company>()
            .AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && !c.IsDeleted && c.ParentCompanyId == company.Id, cancellationToken);

        await Task.WhenAll(contactCountTask, customerCountTask, addressCountTask, childCompanyCountTask);

        return new CompanyWorkspaceDto
        {
            Id = company.Id,
            Name = company.Name,
            Email = _securityService.Mask(nameof(Company), nameof(Company.Email), company.Email),
            Phone = _securityService.Mask(nameof(Company), nameof(Company.Phone), company.Phone),
            Website = company.Website,
            TaxNumber = _securityService.Mask(nameof(Company), nameof(Company.TaxNumber), company.TaxNumber),
            TaxOffice = company.TaxOffice,
            Sector = company.Sector,
            Description = company.Description,
            OwnerUserId = company.OwnerUserId,
            ParentCompanyId = company.ParentCompanyId,
            IsActive = company.IsActive,
            ContactCount = contactCountTask.Result,
            CustomerCount = customerCountTask.Result,
            AddressCount = addressCountTask.Result,
            ChildCompanyCount = childCompanyCountTask.Result,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };
    }
}
