// <copyright file="CustomerManagementMappingExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

internal static class CustomerManagementMappingExtensions
{
    public static AddressDto ToDto(this Address address)
        => new(
            address.Id,
            address.AddressType,
            address.Line1,
            address.Line2,
            address.District,
            address.City,
            address.State,
            address.Country,
            address.ZipCode,
            address.IsDefault,
            Convert.ToBase64String(address.RowVersion));

    public static CompanyDetailDto ToDetailDto(this Company company)
        => new(
            company.Id,
            company.Name,
            company.TaxNumber,
            company.TaxOffice,
            company.Website,
            company.Email,
            company.Phone,
            company.Sector,
            company.EmployeeCountRange,
            company.AnnualRevenue,
            company.Description,
            company.Notes,
            company.CompanyType,
            company.OwnerUserId,
            company.ParentCompanyId,
            company.IsActive,
            company.Addresses.OrderByDescending(x => x.IsDefault).ThenBy(x => x.AddressType).Select(ToDto).ToList(),
            Convert.ToBase64String(company.RowVersion),
            company.LogoUrl);

    public static ContactDetailDto ToDetailDto(this Contact contact)
        => new(
            contact.Id,
            contact.FirstName,
            contact.LastName,
            contact.FullName,
            contact.Title,
            contact.Email,
            contact.MobilePhone,
            contact.WorkPhone,
            contact.PersonalPhone,
            contact.BirthDate,
            contact.Gender,
            contact.Department,
            contact.JobTitle,
            contact.Description,
            contact.Notes,
            contact.OwnerUserId,
            contact.CompanyId,
            contact.Company?.Name,
            contact.CustomerId,
            contact.Customer?.FullName,
            contact.IsPrimaryContact,
            contact.IsActive,
            Convert.ToBase64String(contact.RowVersion));

    public static CustomerDetailDto ToDetailDto(this Customer customer)
        => new(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.FullName,
            customer.Title,
            customer.Email,
            customer.MobilePhone,
            customer.WorkPhone,
            customer.PersonalPhone,
            customer.BirthDate,
            customer.Gender,
            customer.Department,
            customer.JobTitle,
            customer.Description,
            customer.Notes,
            customer.OwnerUserId,
            customer.CustomerType,
            customer.IdentityNumber,
            customer.IsVip,
            customer.CompanyId,
            customer.Company?.Name,
            customer.IsActive,
            customer.Addresses.OrderByDescending(x => x.IsDefault).ThenBy(x => x.AddressType).Select(ToDto).ToList(),
            customer.Contacts
                .OrderByDescending(x => x.IsPrimaryContact)
                .ThenBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(x => new CustomerContactSummaryDto(
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.MobilePhone,
                    x.IsPrimaryContact,
                    x.IsActive))
                .ToList(),
            Convert.ToBase64String(customer.RowVersion),
            customer.ProfileImageUrl);
}
