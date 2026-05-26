// <copyright file="CompanyAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Integration;
using NetMetric.CRM.CustomerManagement.Application.Commands.Companies;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Media;
using NetMetric.Media.Abstractions;
using NetMetric.Media.Models;
using NetMetric.Repository;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class CompanyAdministrationService(
    CustomerManagementDbContext dbContext,
    IRepository<Company, Guid> companyRepository,
    ICurrentUserService currentUserService,
    IMediaAssetService mediaAssetService,
    ICustomerManagementOutbox outbox,
    IGlobalTrashIndexWriter trashIndexWriter)
    : ICompanyAdministrationService
{
    private readonly CustomerManagementDbContext _dbContext = dbContext;
    private readonly IRepository<Company, Guid> _companyRepository = companyRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IMediaAssetService _mediaAssetService = mediaAssetService;
    private readonly ICustomerManagementOutbox _outbox = outbox;
    private readonly IGlobalTrashIndexWriter _trashIndexWriter = trashIndexWriter;

    public async Task<CompanyDetailDto> CreateAsync(CreateCompanyCommand request, CancellationToken cancellationToken = default)
    {
        var exists = await _companyRepository.AnyAsync(
            x => x.TenantId == _currentUserService.TenantId && x.Name == request.Name.Trim(),
            cancellationToken);

        if (exists)
            throw new ConflictAppException("A company with the same name already exists.");

        var entity = new Company
        {
            Name = request.Name.Trim(),
            TaxNumber = TrimToNull(request.TaxNumber),
            TaxOffice = TrimToNull(request.TaxOffice),
            Website = TrimToNull(request.Website),
            Email = TrimToNull(request.Email),
            Phone = TrimToNull(request.Phone),
            Sector = TrimToNull(request.Sector),
            EmployeeCountRange = TrimToNull(request.EmployeeCountRange),
            AnnualRevenue = request.AnnualRevenue,
            Description = TrimToNull(request.Description),
            CompanyType = request.CompanyType,
            OwnerUserId = request.OwnerUserId,
            ParentCompanyId = request.ParentCompanyId
        };

        entity.SetNotes(request.Notes);

        await _companyRepository.AddAsync(entity, cancellationToken);
        await _outbox.EnqueueCompanyCreatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        entity = await LoadCompanyAsync(entity.Id, cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task<CompanyDetailDto> UpdateAsync(UpdateCompanyCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new NotFoundAppException("Company not found.");

        var duplicateExists = await _companyRepository.AnyAsync(
            x => x.TenantId == _currentUserService.TenantId && x.Id != request.CompanyId && x.Name == request.Name.Trim(),
            cancellationToken);

        if (duplicateExists)
            throw new ConflictAppException("A company with the same name already exists.");

        ConcurrencyHelper.ApplyRowVersion(_dbContext, entity, request.RowVersion);

        entity.Name = request.Name.Trim();
        entity.TaxNumber = TrimToNull(request.TaxNumber);
        entity.TaxOffice = TrimToNull(request.TaxOffice);
        entity.Website = TrimToNull(request.Website);
        entity.Email = TrimToNull(request.Email);
        entity.Phone = TrimToNull(request.Phone);
        entity.Sector = TrimToNull(request.Sector);
        entity.EmployeeCountRange = TrimToNull(request.EmployeeCountRange);
        entity.AnnualRevenue = request.AnnualRevenue;
        entity.Description = TrimToNull(request.Description);
        entity.CompanyType = request.CompanyType;
        entity.OwnerUserId = request.OwnerUserId;
        entity.ParentCompanyId = request.ParentCompanyId;
        entity.SetNotes(request.Notes);

        await _outbox.EnqueueCompanyUpdatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        entity = await LoadCompanyAsync(entity.Id, cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task ActivateAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var entity = await _companyRepository.GetByIdAsync(companyId, cancellationToken)
            ?? throw new NotFoundAppException("Company not found.");

        entity.Activate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var entity = await _companyRepository.GetByIdAsync(companyId, cancellationToken)
            ?? throw new NotFoundAppException("Company not found.");

        entity.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CompanyDetailDto> UploadLogoAsync(UploadCompanyLogoCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Companies
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == request.CompanyId, cancellationToken)
            ?? throw new NotFoundAppException("Company not found.");

        MediaUploadResult upload;
        try
        {
            upload = await _mediaAssetService.UploadImageAsync(
                new MediaUploadRequest(
                    _currentUserService.TenantId.ToString(),
                    "company-logo",
                    _currentUserService.UserId.ToString(),
                    request.FileName,
                    request.ContentType,
                    request.Content,
                    request.Length,
                    "customer-management"),
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationAppException(ex.Message);
        }
        catch (MediaValidationException ex)
        {
            throw new ValidationAppException(ex.Message);
        }

        var previousStorageKey = entity.LogoStorageKey;
        entity.SetLogo(upload.StorageKey, upload.PublicUrl, upload.ContentType);

        if (!string.IsNullOrWhiteSpace(previousStorageKey))
        {
            await _mediaAssetService.DeleteAsync(previousStorageKey, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task<CompanyDetailDto> RemoveLogoAsync(RemoveCompanyLogoCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Companies
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == request.CompanyId, cancellationToken)
            ?? throw new NotFoundAppException("Company not found.");

        var previousStorageKey = entity.LogoStorageKey;
        if (!string.IsNullOrWhiteSpace(previousStorageKey))
        {
            await _mediaAssetService.DeleteAsync(previousStorageKey, cancellationToken);
        }

        entity.SetLogo(null, null, null);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task SoftDeleteAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var entity = await _companyRepository.GetByIdAsync(companyId, cancellationToken)
            ?? throw new NotFoundAppException("Company not found.");

        await _trashIndexWriter.AddCompanyDeletionAsync(entity, cancellationToken);
        _companyRepository.Remove(entity);
        await _outbox.EnqueueCompanyDeletedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<Company> LoadCompanyAsync(Guid companyId, CancellationToken cancellationToken)
        => _dbContext.Companies.Include(x => x.Addresses).FirstAsync(x => x.Id == companyId, cancellationToken);

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
