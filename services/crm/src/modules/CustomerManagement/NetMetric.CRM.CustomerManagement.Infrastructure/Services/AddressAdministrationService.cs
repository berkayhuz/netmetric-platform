// <copyright file="AddressAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.Exceptions;
using NetMetric.Repository;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class AddressAdministrationService(
    CustomerManagementDbContext dbContext,
    IRepository<Address, Guid> addressRepository,
    IRepository<Company, Guid> companyRepository,
    IRepository<Customer, Guid> customerRepository)
    : IAddressAdministrationService
{
    private readonly CustomerManagementDbContext _dbContext = dbContext;
    private readonly IRepository<Address, Guid> _addressRepository = addressRepository;
    private readonly IRepository<Company, Guid> _companyRepository = companyRepository;
    private readonly IRepository<Customer, Guid> _customerRepository = customerRepository;

    public async Task<AddressDto> AddToCompanyAsync(AddCompanyAddressCommand request, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new NotFoundAppException("Company not found.");

        var entity = new Address
        {
            CompanyId = company.Id,
            AddressType = request.AddressType,
            Line1 = request.Line1.Trim(),
            Line2 = TrimToNull(request.Line2),
            District = TrimToNull(request.District),
            City = TrimToNull(request.City),
            State = TrimToNull(request.State),
            Country = TrimToNull(request.Country),
            ZipCode = TrimToNull(request.ZipCode),
            IsDefault = request.IsDefault
        };

        await NormalizeDefaultFlagsAsync(entity.CompanyId, null, entity.IsDefault, entity.Id, cancellationToken);
        await _addressRepository.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<AddressDto> AddToCustomerAsync(AddCustomerAddressCommand request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundAppException("Customer not found.");

        var entity = new Address
        {
            CustomerId = customer.Id,
            AddressType = request.AddressType,
            Line1 = request.Line1.Trim(),
            Line2 = TrimToNull(request.Line2),
            District = TrimToNull(request.District),
            City = TrimToNull(request.City),
            State = TrimToNull(request.State),
            Country = TrimToNull(request.Country),
            ZipCode = TrimToNull(request.ZipCode),
            IsDefault = request.IsDefault
        };

        await NormalizeDefaultFlagsAsync(null, entity.CustomerId, entity.IsDefault, entity.Id, cancellationToken);
        await _addressRepository.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<AddressDto> UpdateAsync(UpdateAddressCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Addresses.FirstOrDefaultAsync(x => x.Id == request.AddressId, cancellationToken)
            ?? throw new NotFoundAppException("Address not found.");

        ConcurrencyHelper.ApplyRowVersion(_dbContext, entity, request.RowVersion);

        entity.AddressType = request.AddressType;
        entity.Line1 = request.Line1.Trim();
        entity.Line2 = TrimToNull(request.Line2);
        entity.District = TrimToNull(request.District);
        entity.City = TrimToNull(request.City);
        entity.State = TrimToNull(request.State);
        entity.Country = TrimToNull(request.Country);
        entity.ZipCode = TrimToNull(request.ZipCode);
        entity.IsDefault = request.IsDefault;

        await NormalizeDefaultFlagsAsync(entity.CompanyId, entity.CustomerId, entity.IsDefault, entity.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task SetDefaultAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Addresses.FirstOrDefaultAsync(x => x.Id == addressId, cancellationToken)
            ?? throw new NotFoundAppException("Address not found.");

        entity.IsDefault = true;
        await NormalizeDefaultFlagsAsync(entity.CompanyId, entity.CustomerId, true, entity.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        var entity = await _addressRepository.GetByIdAsync(addressId, cancellationToken)
            ?? throw new NotFoundAppException("Address not found.");

        _addressRepository.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task NormalizeDefaultFlagsAsync(
        Guid? companyId,
        Guid? customerId,
        bool isDefault,
        Guid currentAddressId,
        CancellationToken cancellationToken)
    {
        if (!isDefault)
            return;

        if (companyId.HasValue)
        {
            var companyAddresses = await _dbContext.Addresses
                .Where(x => x.CompanyId == companyId && x.Id != currentAddressId)
                .ToListAsync(cancellationToken);

            companyAddresses.ForEach(x => x.IsDefault = false);
        }

        if (customerId.HasValue)
        {
            var customerAddresses = await _dbContext.Addresses
                .Where(x => x.CustomerId == customerId && x.Id != currentAddressId)
                .ToListAsync(cancellationToken);

            customerAddresses.ForEach(x => x.IsDefault = false);
        }
    }

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
