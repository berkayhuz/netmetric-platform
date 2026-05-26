// <copyright file="GetCustomerByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Customers;

public sealed class GetCustomerByIdQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto?>
{
    public async Task<CustomerDetailDto?> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.CustomersResource);
        var canSeeContactData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CustomersResource, "contactData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeIdentityNumber = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CustomersResource, "identityNumber", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeNotes = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CustomersResource, "notes", scope.Permissions)
            .Visibility == FieldVisibility.Visible;

        var customer = await dbContext.Customers
            .AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Addresses)
            .Include(x => x.Contacts)
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId, cancellationToken);

        if (customer is null)
            return null;

        return new CustomerDetailDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.FullName,
            customer.Title,
            canSeeContactData ? customer.Email : null,
            canSeeContactData ? customer.MobilePhone : null,
            canSeeContactData ? customer.WorkPhone : null,
            canSeeContactData ? customer.PersonalPhone : null,
            customer.BirthDate,
            customer.Gender,
            customer.Department,
            customer.JobTitle,
            customer.Description,
            canSeeNotes ? customer.Notes : null,
            customer.OwnerUserId,
            customer.CustomerType,
            canSeeIdentityNumber ? customer.IdentityNumber : null,
            customer.IsVip,
            customer.CompanyId,
            customer.Company?.Name,
            customer.IsActive,
            customer.Addresses
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
            customer.Contacts
                .OrderByDescending(x => x.IsPrimaryContact)
                .ThenBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(x => new CustomerContactSummaryDto(
                    x.Id,
                    x.FullName,
                    canSeeContactData ? x.Email : null,
                    canSeeContactData ? x.MobilePhone : null,
                    x.IsPrimaryContact,
                    x.IsActive))
                .ToList(),
            Convert.ToBase64String(customer.RowVersion),
            customer.ProfileImageUrl);
    }
}
