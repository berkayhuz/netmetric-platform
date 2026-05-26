// <copyright file="CustomerImportWizardCommands.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomerOperations;

public sealed record CustomerImportRawRow(IReadOnlyDictionary<string, string?> Values);
public sealed record CustomerImportRowDto(Guid Id, int RowNumber, CustomerImportRowStatus Status, Guid? ImportedEntityId, string? ValidationErrorsJson, string? DuplicateWarningsJson, string? MappedDataJson);
public sealed record CustomerImportBatchDto(Guid Id, string FileName, string Source, CustomerImportBatchStatus Status, int TotalRows, int ValidRows, int InvalidRows, int DuplicateRows, DateTime CreatedAt, DateTime? CompletedAt, IReadOnlyList<CustomerImportRowDto> Rows);

public sealed record CreateCustomerImportBatchCommand(string FileName, string Source, IReadOnlyList<CustomerImportRawRow> Rows) : IRequest<Guid>;
public sealed record PreviewCustomerImportMappingCommand(Guid BatchId) : IRequest<CustomerImportBatchDto>;
public sealed record ValidateCustomerImportBatchCommand(Guid BatchId) : IRequest<CustomerImportBatchDto>;
public sealed record CommitCustomerImportBatchCommand(Guid BatchId, CustomerDuplicateStrategy DuplicateStrategy) : IRequest<CustomerImportBatchDto>;
public sealed record GetCustomerImportBatchQuery(Guid BatchId) : IRequest<CustomerImportBatchDto>;
public sealed record ListCustomerImportBatchesQuery(int Take = 50) : IRequest<IReadOnlyList<CustomerImportBatchDto>>;
public sealed record CancelCustomerImportBatchCommand(Guid BatchId, string? Reason) : IRequest;

public sealed class CreateCustomerImportBatchCommandValidator : AbstractValidator<CreateCustomerImportBatchCommand>
{
    public CreateCustomerImportBatchCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.Source).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= 5000).WithMessage("Import row count must be between 1 and 5000.");
    }
}

public sealed class CreateCustomerImportBatchCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<CreateCustomerImportBatchCommand, Guid>
{
    public async Task<Guid> Handle(CreateCustomerImportBatchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var batch = new CustomerImportBatch
        {
            TenantId = tenantId,
            FileName = request.FileName.Trim(),
            Source = request.Source.Trim(),
            Status = CustomerImportBatchStatus.Uploaded,
            TotalRows = request.Rows.Count
        };

        var rowNumber = 1;
        foreach (var row in request.Rows)
        {
            batch.Rows.Add(new CustomerImportRow
            {
                TenantId = tenantId,
                RowNumber = rowNumber++,
                RawDataJson = JsonSerializer.Serialize(row.Values),
                Status = CustomerImportRowStatus.Pending
            });
        }

        await dbContext.CustomerImportBatches.AddAsync(batch, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return batch.Id;
    }
}

public sealed class PreviewCustomerImportMappingCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<PreviewCustomerImportMappingCommand, CustomerImportBatchDto>
{
    public async Task<CustomerImportBatchDto> Handle(PreviewCustomerImportMappingCommand request, CancellationToken cancellationToken)
    {
        var batch = await CustomerImportBatchProjection.LoadBatchAsync(dbContext, currentUserService.EnsureTenant(), request.BatchId, cancellationToken);
        foreach (var row in batch.Rows.Where(x => x.Status == CustomerImportRowStatus.Pending))
        {
            row.MappedDataJson = JsonSerializer.Serialize(CustomerImportMapper.Map(CustomerImportBatchProjection.ReadRaw(row.RawDataJson)));
        }

        batch.Status = CustomerImportBatchStatus.Mapped;
        await dbContext.SaveChangesAsync(cancellationToken);
        return CustomerImportBatchProjection.ToDto(batch);
    }
}

public sealed class ValidateCustomerImportBatchCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, IDuplicateDetectionService duplicateDetectionService) : IRequestHandler<ValidateCustomerImportBatchCommand, CustomerImportBatchDto>
{
    public async Task<CustomerImportBatchDto> Handle(ValidateCustomerImportBatchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var batch = await CustomerImportBatchProjection.LoadBatchAsync(dbContext, tenantId, request.BatchId, cancellationToken);
        var existing = await dbContext.Customers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Email, x.MobilePhone, x.WorkPhone, x.PersonalPhone })
            .ToListAsync(cancellationToken);

        foreach (var row in batch.Rows)
        {
            var mapped = CustomerImportMapper.Map(CustomerImportBatchProjection.ReadRaw(row.RawDataJson));
            row.MappedDataJson = JsonSerializer.Serialize(mapped);
            var errors = CustomerImportMapper.Validate(mapped);
            var email = duplicateDetectionService.NormalizeEmail(mapped.Email);
            var phone = duplicateDetectionService.NormalizePhone(mapped.Phone);
            var duplicates = existing
                .Where(x => (email is not null && email == duplicateDetectionService.NormalizeEmail(x.Email)) ||
                            (phone is not null && phone == duplicateDetectionService.NormalizePhone(x.MobilePhone ?? x.WorkPhone ?? x.PersonalPhone)))
                .Select(x => x.Id)
                .ToList();

            row.ValidationErrorsJson = errors.Count == 0 ? null : JsonSerializer.Serialize(errors);
            row.DuplicateWarningsJson = duplicates.Count == 0 ? null : JsonSerializer.Serialize(duplicates);
            row.Status = errors.Count > 0 ? CustomerImportRowStatus.Invalid : duplicates.Count > 0 ? CustomerImportRowStatus.Duplicate : CustomerImportRowStatus.Valid;
        }

        batch.ValidRows = batch.Rows.Count(x => x.Status is CustomerImportRowStatus.Valid or CustomerImportRowStatus.Duplicate);
        batch.InvalidRows = batch.Rows.Count(x => x.Status == CustomerImportRowStatus.Invalid);
        batch.DuplicateRows = batch.Rows.Count(x => x.Status == CustomerImportRowStatus.Duplicate);
        batch.Status = CustomerImportBatchStatus.Validated;
        await dbContext.SaveChangesAsync(cancellationToken);
        return CustomerImportBatchProjection.ToDto(batch);
    }
}

public sealed class CommitCustomerImportBatchCommandHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IDuplicateDetectionService duplicateDetectionService,
    ICustomerAuditTrailWriter auditWriter,
    ICustomerSearchIndexer searchIndexer,
    ICustomerDataQualityService dataQualityService) : IRequestHandler<CommitCustomerImportBatchCommand, CustomerImportBatchDto>
{
    public async Task<CustomerImportBatchDto> Handle(CommitCustomerImportBatchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var batch = await CustomerImportBatchProjection.LoadBatchAsync(dbContext, tenantId, request.BatchId, cancellationToken);
        if (batch.Status != CustomerImportBatchStatus.Validated && batch.Status != CustomerImportBatchStatus.Importing && batch.Status != CustomerImportBatchStatus.Completed)
            throw new ValidationAppException("Import batch must be validated before commit.");

        batch.Status = CustomerImportBatchStatus.Importing;
        foreach (var row in batch.Rows.Where(x => x.Status is CustomerImportRowStatus.Valid or CustomerImportRowStatus.Duplicate))
        {
            if (row.ImportedEntityId.HasValue || row.Status == CustomerImportRowStatus.Imported || row.Status == CustomerImportRowStatus.Skipped)
                continue;

            var mapped = CustomerImportMapper.Map(CustomerImportBatchProjection.ReadRaw(row.RawDataJson));
            var existing = await FindExistingAsync(tenantId, mapped, duplicateDetectionService, cancellationToken);
            if (existing is not null && request.DuplicateStrategy == CustomerDuplicateStrategy.Skip)
            {
                row.Status = CustomerImportRowStatus.Skipped;
                row.ImportedEntityId = existing.Id;
                continue;
            }

            var customer = existing is not null && request.DuplicateStrategy != CustomerDuplicateStrategy.CreateNew
                ? Apply(existing, mapped)
                : await CreateAsync(tenantId, mapped, cancellationToken);

            row.Status = CustomerImportRowStatus.Imported;
            row.ImportedEntityId = customer.Id;
            await WriteConsentIfPresentAsync(tenantId, customer.Id, mapped, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditWriter.WriteAsync(CustomerEntityType.Customer, customer.Id, CustomerAuditAction.Imported, null, null, null, JsonSerializer.Serialize(new { batchId = batch.Id, rowNumber = row.RowNumber, request.DuplicateStrategy }), cancellationToken);
            await searchIndexer.ReindexCustomerAsync(customer.Id, cancellationToken);
            await UpsertQualityAsync(customer, cancellationToken);
        }

        batch.ValidRows = batch.Rows.Count(x => x.Status is CustomerImportRowStatus.Imported or CustomerImportRowStatus.Skipped);
        batch.InvalidRows = batch.Rows.Count(x => x.Status == CustomerImportRowStatus.Invalid);
        batch.DuplicateRows = batch.Rows.Count(x => x.DuplicateWarningsJson != null);
        batch.Status = CustomerImportBatchStatus.Completed;
        batch.CompletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return CustomerImportBatchProjection.ToDto(batch);
    }

    private async Task<Customer?> FindExistingAsync(Guid tenantId, CustomerImportMappedRow mapped, IDuplicateDetectionService duplicateDetectionService, CancellationToken cancellationToken)
    {
        var email = duplicateDetectionService.NormalizeEmail(mapped.Email);
        var phone = duplicateDetectionService.NormalizePhone(mapped.Phone);
        var customers = await dbContext.Customers
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Take(500)
            .ToListAsync(cancellationToken);

        return customers.FirstOrDefault(x =>
            (email is not null && email == duplicateDetectionService.NormalizeEmail(x.Email)) ||
            (phone is not null && phone == duplicateDetectionService.NormalizePhone(x.MobilePhone ?? x.WorkPhone ?? x.PersonalPhone)));
    }

    private async Task<Customer> CreateAsync(Guid tenantId, CustomerImportMappedRow mapped, CancellationToken cancellationToken)
    {
        var customer = Apply(new Customer { TenantId = tenantId }, mapped);
        await dbContext.Customers.AddAsync(customer, cancellationToken);
        return customer;
    }

    private static Customer Apply(Customer customer, CustomerImportMappedRow mapped)
    {
        customer.FirstName = mapped.FirstName;
        customer.LastName = mapped.LastName;
        customer.Email = mapped.Email;
        customer.MobilePhone = mapped.Phone;
        customer.OwnerUserId = mapped.OwnerUserId;
        customer.CustomerType = CustomerType.Individual;
        return customer;
    }

    private async Task WriteConsentIfPresentAsync(Guid tenantId, Guid customerId, CustomerImportMappedRow mapped, CancellationToken cancellationToken)
    {
        if (!mapped.MarketingConsent.HasValue)
        {
            return;
        }

        var consent = await dbContext.CustomerConsents.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customerId && x.Channel == CustomerConsentChannel.Email && x.Purpose == CustomerConsentPurpose.Marketing, cancellationToken);
        var created = consent is null;
        consent ??= new CustomerConsent { TenantId = tenantId, EntityType = CustomerEntityType.Customer, EntityId = customerId, Channel = CustomerConsentChannel.Email, Purpose = CustomerConsentPurpose.Marketing, ValidFromUtc = DateTime.UtcNow, Source = CustomerConsentSource.Import };
        var previous = consent.Status;
        consent.Status = mapped.MarketingConsent.Value ? CustomerConsentStatus.OptedIn : CustomerConsentStatus.OptedOut;
        if (created)
        {
            await dbContext.CustomerConsents.AddAsync(consent, cancellationToken);
        }
        await dbContext.CustomerConsentHistories.AddAsync(new CustomerConsentHistory { TenantId = tenantId, ConsentId = consent.Id, PreviousStatus = previous, NewStatus = consent.Status, ChangedAtUtc = DateTime.UtcNow, Source = CustomerConsentSource.Import, Reason = "Import" }, cancellationToken);
    }

    private async Task UpsertQualityAsync(Customer customer, CancellationToken cancellationToken)
    {
        var snapshot = dataQualityService.Calculate(customer, [], 0, null);
        var existing = await dbContext.CustomerDataQualitySnapshots.FirstOrDefaultAsync(x => x.TenantId == customer.TenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id, cancellationToken);
        if (existing is null)
        {
            await dbContext.CustomerDataQualitySnapshots.AddAsync(snapshot, cancellationToken);
        }
        else
        {
            existing.Score = snapshot.Score;
            existing.MissingFieldsJson = snapshot.MissingFieldsJson;
            existing.InvalidFieldsJson = snapshot.InvalidFieldsJson;
            existing.RecommendationsJson = snapshot.RecommendationsJson;
            existing.CalculatedAtUtc = snapshot.CalculatedAtUtc;
        }
    }
}

public sealed class GetCustomerImportBatchQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetCustomerImportBatchQuery, CustomerImportBatchDto>
{
    public async Task<CustomerImportBatchDto> Handle(GetCustomerImportBatchQuery request, CancellationToken cancellationToken)
        => CustomerImportBatchProjection.ToDto(await CustomerImportBatchProjection.LoadBatchAsync(dbContext, currentUserService.EnsureTenant(), request.BatchId, cancellationToken));
}

public sealed class ListCustomerImportBatchesQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<ListCustomerImportBatchesQuery, IReadOnlyList<CustomerImportBatchDto>>
{
    public async Task<IReadOnlyList<CustomerImportBatchDto>> Handle(ListCustomerImportBatchesQuery request, CancellationToken cancellationToken)
    {
        var batches = await dbContext.CustomerImportBatches.AsNoTracking()
            .Where(x => x.TenantId == currentUserService.EnsureTenant() && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(request.Take, 1, 100))
            .ToListAsync(cancellationToken);
        return batches.Select(CustomerImportBatchProjection.ToDto).ToList();
    }
}

public sealed class CancelCustomerImportBatchCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<CancelCustomerImportBatchCommand>
{
    public async Task Handle(CancelCustomerImportBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await CustomerImportBatchProjection.LoadBatchAsync(dbContext, currentUserService.EnsureTenant(), request.BatchId, cancellationToken);
        if (batch.Status == CustomerImportBatchStatus.Completed)
            throw new ValidationAppException("Completed import batches cannot be cancelled.");
        batch.Status = CustomerImportBatchStatus.Cancelled;
        batch.CompletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed record CustomerImportMappedRow(string FirstName, string LastName, string? Email, string? Phone, Guid? OwnerUserId, CustomerLifecycleStage? LifecycleStage, bool? MarketingConsent);

internal static class CustomerImportMapper
{
    public static CustomerImportMappedRow Map(IReadOnlyDictionary<string, string?> row)
    {
        var first = Get(row, "contact first name", "first name", "firstname");
        var last = Get(row, "contact last name", "last name", "lastname");
        var full = Get(row, "customer name", "name", "full name");
        if ((string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last)) && !string.IsNullOrWhiteSpace(full))
        {
            var parts = full.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            first = string.IsNullOrWhiteSpace(first) ? parts.ElementAtOrDefault(0) : first;
            last = string.IsNullOrWhiteSpace(last) ? parts.ElementAtOrDefault(1) ?? "-" : last;
        }

        Guid? owner = Guid.TryParse(Get(row, "owner", "ownerUserId", "owner user id"), out var ownerId) ? ownerId : null;
        CustomerLifecycleStage? stage = Enum.TryParse<CustomerLifecycleStage>(Get(row, "lifecycle stage", "stage"), true, out var parsedStage) ? parsedStage : null;
        bool? consent = bool.TryParse(Get(row, "marketing consent", "email consent", "consent"), out var parsedConsent) ? parsedConsent : null;
        return new CustomerImportMappedRow(first ?? string.Empty, last ?? string.Empty, Normalize(Get(row, "email")), Normalize(Get(row, "phone", "mobile phone")), owner, stage, consent);
    }

    public static IReadOnlyList<string> Validate(CustomerImportMappedRow row)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(row.FirstName)) errors.Add("FirstName is required.");
        if (string.IsNullOrWhiteSpace(row.LastName)) errors.Add("LastName is required.");
        if (string.IsNullOrWhiteSpace(row.Email) && string.IsNullOrWhiteSpace(row.Phone)) errors.Add("Email or phone is required.");
        if (!string.IsNullOrWhiteSpace(row.Email) && !row.Email.Contains('@', StringComparison.Ordinal)) errors.Add("Email is invalid.");
        if (!string.IsNullOrWhiteSpace(row.Phone) && row.Phone.Count(char.IsDigit) < 7) errors.Add("Phone is invalid.");
        return errors;
    }

    private static string? Get(IReadOnlyDictionary<string, string?> row, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = row.FirstOrDefault(x => string.Equals(NormalizeKey(x.Key), NormalizeKey(key), StringComparison.Ordinal));
            if (!string.IsNullOrWhiteSpace(match.Value)) return match.Value;
        }
        return null;
    }

    private static string NormalizeKey(string value) => value.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("_", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

internal static class CustomerImportBatchProjection
{
    public static CustomerImportBatchDto ToDto(CustomerImportBatch batch)
        => new(batch.Id, batch.FileName, batch.Source, batch.Status, batch.TotalRows, batch.ValidRows, batch.InvalidRows, batch.DuplicateRows, batch.CreatedAt, batch.CompletedAt, batch.Rows.OrderBy(x => x.RowNumber).Select(x => new CustomerImportRowDto(x.Id, x.RowNumber, x.Status, x.ImportedEntityId, x.ValidationErrorsJson, x.DuplicateWarningsJson, x.MappedDataJson)).ToList());

    public static async Task<CustomerImportBatch> LoadBatchAsync(ICustomerManagementDbContext dbContext, Guid tenantId, Guid batchId, CancellationToken cancellationToken)
        => await dbContext.CustomerImportBatches.Include(x => x.Rows).FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == batchId, cancellationToken)
           ?? throw new NotFoundAppException("Customer import batch was not found.");

    public static IReadOnlyDictionary<string, string?> ReadRaw(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string?>>(json) ?? new Dictionary<string, string?>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string?>();
        }
    }
}
