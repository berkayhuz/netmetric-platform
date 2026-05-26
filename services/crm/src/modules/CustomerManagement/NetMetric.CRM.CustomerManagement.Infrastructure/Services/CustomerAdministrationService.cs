// <copyright file="CustomerAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Auditing;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Integration;
using NetMetric.CRM.CustomerManagement.Application.Commands.Customers;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Media;
using NetMetric.Media.Abstractions;
using NetMetric.Media.Models;
using NetMetric.Repository;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class CustomerAdministrationService(
    CustomerManagementDbContext dbContext,
    IRepository<Customer, Guid> customerRepository,
    ICustomerManagementOutbox outbox,
    ICurrentUserService currentUserService,
    IMediaAssetService mediaAssetService,
    IGlobalTrashIndexWriter trashIndexWriter)
    : ICustomerAdministrationService
{
    private readonly CustomerManagementDbContext _dbContext = dbContext;
    private readonly IRepository<Customer, Guid> _customerRepository = customerRepository;
    private readonly ICustomerManagementOutbox _outbox = outbox;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IMediaAssetService _mediaAssetService = mediaAssetService;
    private readonly IGlobalTrashIndexWriter _trashIndexWriter = trashIndexWriter;

    public async Task<CustomerDetailDto> CreateAsync(CreateCustomerCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Customer
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Title = TrimToNull(request.Title),
            Email = TrimToNull(request.Email),
            MobilePhone = TrimToNull(request.MobilePhone),
            WorkPhone = TrimToNull(request.WorkPhone),
            PersonalPhone = TrimToNull(request.PersonalPhone),
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            Department = TrimToNull(request.Department),
            JobTitle = TrimToNull(request.JobTitle),
            Description = TrimToNull(request.Description),
            OwnerUserId = request.OwnerUserId,
            CustomerType = request.CustomerType,
            IdentityNumber = TrimToNull(request.IdentityNumber),
            IsVip = request.IsVip,
            CompanyId = request.CompanyId
        };
        entity.SetActive(request.IsActive);

        entity.SetNotes(request.Notes);

        await _customerRepository.AddAsync(entity, cancellationToken);
        AddAuditLog(entity.Id, "Created", CollectCreateColumns(request));
        await _outbox.EnqueueCustomerCreatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        entity = await LoadCustomerAsync(entity.Id, cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task<CustomerDetailDto> UpdateAsync(UpdateCustomerCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundAppException("Customer not found.");

        ConcurrencyHelper.ApplyRowVersion(_dbContext, entity, request.RowVersion);

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Title = TrimToNull(request.Title);
        entity.Email = TrimToNull(request.Email);
        entity.MobilePhone = TrimToNull(request.MobilePhone);
        entity.WorkPhone = TrimToNull(request.WorkPhone);
        entity.PersonalPhone = TrimToNull(request.PersonalPhone);
        entity.BirthDate = request.BirthDate;
        entity.Gender = request.Gender;
        entity.Department = TrimToNull(request.Department);
        entity.JobTitle = TrimToNull(request.JobTitle);
        entity.Description = TrimToNull(request.Description);
        entity.OwnerUserId = request.OwnerUserId;
        entity.CustomerType = request.CustomerType;
        entity.IdentityNumber = TrimToNull(request.IdentityNumber);
        entity.IsVip = request.IsVip;
        entity.SetActive(request.IsActive);
        entity.CompanyId = request.CompanyId;
        entity.SetNotes(request.Notes);

        AddAuditLog(entity.Id, "Updated", CollectUpdateColumns());
        await _outbox.EnqueueCustomerUpdatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        entity = await LoadCustomerAsync(entity.Id, cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task MarkVipAsync(Guid customerId, bool isVip, CancellationToken cancellationToken = default)
    {
        var entity = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundAppException("Customer not found.");

        entity.IsVip = isVip;
        AddAuditLog(entity.Id, "Updated", [nameof(Customer.IsVip)]);
        await _outbox.EnqueueCustomerUpdatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerDetailDto> UploadImageAsync(UploadCustomerImageCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers
            .Include(x => x.Company)
            .Include(x => x.Addresses)
            .Include(x => x.Contacts)
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundAppException("Customer not found.");

        MediaUploadResult upload;
        try
        {
            upload = await _mediaAssetService.UploadImageAsync(
                new MediaUploadRequest(
                    _currentUserService.TenantId.ToString(),
                    "customer-profile-image",
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

        var previousStorageKey = entity.ProfileImageStorageKey;
        entity.SetProfileImage(upload.StorageKey, upload.PublicUrl, upload.ContentType);

        if (!string.IsNullOrWhiteSpace(previousStorageKey))
        {
            await _mediaAssetService.DeleteAsync(previousStorageKey, cancellationToken);
        }

        AddAuditLog(entity.Id, "Updated", [nameof(Customer.ProfileImageUrl)]);
        await _outbox.EnqueueCustomerUpdatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDetailDto();
    }

    public async Task<CustomerDetailDto> RemoveImageAsync(RemoveCustomerImageCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers
            .Include(x => x.Company)
            .Include(x => x.Addresses)
            .Include(x => x.Contacts)
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundAppException("Customer not found.");

        var previousStorageKey = entity.ProfileImageStorageKey;
        if (!string.IsNullOrWhiteSpace(previousStorageKey))
        {
            await _mediaAssetService.DeleteAsync(previousStorageKey, cancellationToken);
        }

        entity.SetProfileImage(null, null, null);
        AddAuditLog(entity.Id, "Updated", [nameof(Customer.ProfileImageUrl)]);
        await _outbox.EnqueueCustomerUpdatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDetailDto();
    }

    public async Task SoftDeleteAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundAppException("Customer not found.");

        await _trashIndexWriter.AddCustomerDeletionAsync(entity, cancellationToken);
        _customerRepository.Remove(entity);
        AddAuditLog(entity.Id, "Deleted", [nameof(Customer.IsDeleted), nameof(Customer.DeletedAt), nameof(Customer.DeletedBy)]);
        await _outbox.EnqueueCustomerDeletedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<Customer> LoadCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        => _dbContext.Customers
            .Include(x => x.Company)
            .Include(x => x.Addresses)
            .Include(x => x.Contacts)
            .FirstAsync(x => x.Id == customerId, cancellationToken);

    private void AddAuditLog(Guid customerId, string actionType, IReadOnlyCollection<string> changedColumns)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            TenantId = _currentUserService.TenantId,
            EntityName = nameof(Customer),
            EntityId = customerId,
            ActionType = actionType,
            ChangedColumnsJson = JsonSerializer.Serialize(changedColumns),
            CorrelationId = Activity.Current?.TraceId.ToString(),
            ChangedByUserId = _currentUserService.UserId,
            ChangedAt = DateTime.UtcNow
        });
    }

    private static IReadOnlyCollection<string> CollectCreateColumns(CreateCustomerCommand request)
    {
        var columns = new List<string>
        {
            nameof(Customer.FirstName),
            nameof(Customer.LastName),
            nameof(Customer.CustomerType),
            nameof(Customer.IsVip),
            nameof(Customer.IsActive)
        };

        AddOptionalColumn(columns, request.Title, nameof(Customer.Title));
        AddOptionalColumn(columns, request.Email, nameof(Customer.Email));
        AddOptionalColumn(columns, request.MobilePhone, nameof(Customer.MobilePhone));
        AddOptionalColumn(columns, request.WorkPhone, nameof(Customer.WorkPhone));
        AddOptionalColumn(columns, request.PersonalPhone, nameof(Customer.PersonalPhone));
        AddOptionalColumn(columns, request.Department, nameof(Customer.Department));
        AddOptionalColumn(columns, request.JobTitle, nameof(Customer.JobTitle));
        AddOptionalColumn(columns, request.Description, nameof(Customer.Description));
        AddOptionalColumn(columns, request.Notes, nameof(Customer.Notes));
        AddOptionalColumn(columns, request.IdentityNumber, nameof(Customer.IdentityNumber));

        if (request.BirthDate.HasValue)
        {
            columns.Add(nameof(Customer.BirthDate));
        }

        if (request.OwnerUserId.HasValue)
        {
            columns.Add(nameof(Customer.OwnerUserId));
        }

        if (request.CompanyId.HasValue)
        {
            columns.Add(nameof(Customer.CompanyId));
        }

        return columns;
    }

    private static IReadOnlyCollection<string> CollectUpdateColumns() =>
        [
            nameof(Customer.FirstName),
            nameof(Customer.LastName),
            nameof(Customer.Title),
            nameof(Customer.Email),
            nameof(Customer.MobilePhone),
            nameof(Customer.WorkPhone),
            nameof(Customer.PersonalPhone),
            nameof(Customer.BirthDate),
            nameof(Customer.Gender),
            nameof(Customer.Department),
            nameof(Customer.JobTitle),
            nameof(Customer.Description),
            nameof(Customer.Notes),
            nameof(Customer.OwnerUserId),
            nameof(Customer.CustomerType),
            nameof(Customer.IdentityNumber),
            nameof(Customer.IsVip),
            nameof(Customer.IsActive),
            nameof(Customer.CompanyId)
        ];

    private static void AddOptionalColumn(ICollection<string> columns, string? value, string columnName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            columns.Add(columnName);
        }
    }

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
