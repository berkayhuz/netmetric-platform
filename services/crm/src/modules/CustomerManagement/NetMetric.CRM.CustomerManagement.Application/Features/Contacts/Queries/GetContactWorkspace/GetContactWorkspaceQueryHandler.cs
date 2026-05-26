// <copyright file="GetContactWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Contacts;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Contacts.Queries.GetContactWorkspace;

public sealed class GetContactWorkspaceQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService) : IRequestHandler<GetContactWorkspaceQuery, ContactWorkspaceDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<ContactWorkspaceDto> Handle(GetContactWorkspaceQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;

        var contact = await _securityService.ApplyContactReadScope(_dbContext.Set<Contact>())
            .AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Customer)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == request.ContactId)
            .Select(x => new ContactWorkspaceDto
            {
                Id = x.Id,
                FullName = (x.FirstName + " " + x.LastName).Trim(),
                Title = x.Title,
                JobTitle = x.JobTitle,
                Department = x.Department,
                Email = x.Email,
                MobilePhone = x.MobilePhone,
                WorkPhone = x.WorkPhone,
                PersonalPhone = x.PersonalPhone,
                Description = x.Description,
                CompanyId = x.CompanyId,
                CompanyName = x.Company != null ? x.Company.Name : null,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer != null ? x.Customer.FirstName + " " + x.Customer.LastName : null,
                OwnerUserId = x.OwnerUserId,
                IsPrimaryContact = x.IsPrimaryContact,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (contact is null)
            throw new NotFoundAppException("Contact not found.");

        return new ContactWorkspaceDto
        {
            Id = contact.Id,
            FullName = contact.FullName,
            Title = contact.Title,
            JobTitle = contact.JobTitle,
            Department = contact.Department,
            Email = _securityService.Mask(nameof(Contact), nameof(Contact.Email), contact.Email),
            MobilePhone = _securityService.Mask(nameof(Contact), nameof(Contact.MobilePhone), contact.MobilePhone),
            WorkPhone = _securityService.Mask(nameof(Contact), nameof(Contact.WorkPhone), contact.WorkPhone),
            PersonalPhone = _securityService.Mask(nameof(Contact), nameof(Contact.PersonalPhone), contact.PersonalPhone),
            Description = contact.Description,
            CompanyId = contact.CompanyId,
            CompanyName = contact.CompanyName,
            CustomerId = contact.CustomerId,
            CustomerName = contact.CustomerName,
            OwnerUserId = contact.OwnerUserId,
            IsPrimaryContact = contact.IsPrimaryContact,
            IsActive = contact.IsActive,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }
}
