// <copyright file="GetContactByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;

public sealed class GetContactByIdQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetContactByIdQuery, ContactDetailDto?>
{
    public async Task<ContactDetailDto?> Handle(
        GetContactByIdQuery request,
        CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.ContactsResource);
        var canSeeContactData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.ContactsResource, "contactData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeNotes = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.ContactsResource, "notes", scope.Permissions)
            .Visibility == FieldVisibility.Visible;

        var contact = await dbContext.Contacts
            .AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Customer)
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.ContactId, cancellationToken);

        if (contact is null)
            return null;

        return new ContactDetailDto(
            contact.Id,
            contact.FirstName,
            contact.LastName,
            contact.FullName,
            contact.Title,
            canSeeContactData ? contact.Email : null,
            canSeeContactData ? contact.MobilePhone : null,
            canSeeContactData ? contact.WorkPhone : null,
            canSeeContactData ? contact.PersonalPhone : null,
            contact.BirthDate,
            contact.Gender,
            contact.Department,
            contact.JobTitle,
            contact.Description,
            canSeeNotes ? contact.Notes : null,
            contact.OwnerUserId,
            contact.CompanyId,
            contact.Company?.Name,
            contact.CustomerId,
            contact.Customer?.FullName,
            contact.IsPrimaryContact,
            contact.IsActive,
            Convert.ToBase64String(contact.RowVersion));
    }
}
