// <copyright file="GetCustomerWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Customers;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Customers.Queries.GetCustomerWorkspace;

public sealed class GetCustomerWorkspaceQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService) : IRequestHandler<GetCustomerWorkspaceQuery, CustomerWorkspaceDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<CustomerWorkspaceDto> Handle(GetCustomerWorkspaceQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;

        var customer = await _securityService.ApplyCustomerReadScope(_dbContext.Set<Customer>())
            .AsNoTracking()
            .Include(x => x.Company)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == request.CustomerId)
            .Select(x => new CustomerWorkspaceDto
            {
                Id = x.Id,
                FullName = (x.FirstName + " " + x.LastName).Trim(),
                Email = x.Email,
                MobilePhone = x.MobilePhone,
                WorkPhone = x.WorkPhone,
                IdentityNumber = x.IdentityNumber,
                Description = x.Description,
                CompanyId = x.CompanyId,
                CompanyName = x.Company != null ? x.Company.Name : null,
                OwnerUserId = x.OwnerUserId,
                IsVip = x.IsVip,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
            throw new NotFoundAppException("Customer not found.");

        var addressCount = await _dbContext.Set<Address>()
            .AsNoTracking()
            .CountAsync(a => a.TenantId == tenantId && !a.IsDeleted && a.CustomerId == customer.Id, cancellationToken);

        return new CustomerWorkspaceDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = _securityService.Mask(nameof(Customer), nameof(Customer.Email), customer.Email),
            MobilePhone = _securityService.Mask(nameof(Customer), nameof(Customer.MobilePhone), customer.MobilePhone),
            WorkPhone = _securityService.Mask(nameof(Customer), nameof(Customer.WorkPhone), customer.WorkPhone),
            IdentityNumber = _securityService.Mask(nameof(Customer), nameof(Customer.IdentityNumber), customer.IdentityNumber),
            Description = customer.Description,
            CompanyId = customer.CompanyId,
            CompanyName = customer.CompanyName,
            OwnerUserId = customer.OwnerUserId,
            IsVip = customer.IsVip,
            IsActive = customer.IsActive,
            AddressCount = addressCount,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
