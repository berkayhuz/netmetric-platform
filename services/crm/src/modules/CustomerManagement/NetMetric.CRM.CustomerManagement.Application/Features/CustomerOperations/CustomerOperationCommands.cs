// <copyright file="CustomerOperationCommands.cs" company="NetMetric">
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
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomerOperations;

public sealed record UpsertCustomerConsentCommand(Guid CustomerId, CustomerConsentChannel Channel, CustomerConsentPurpose Purpose, CustomerConsentStatus Status, CustomerConsentSource Source, DateTime? ValidUntilUtc, string? EvidenceText, string? EvidenceIpAddress, string? EvidenceUserAgent, string? Reason) : IRequest<Guid>;
public sealed record RevokeCustomerConsentCommand(Guid CustomerId, Guid ConsentId, string Reason) : IRequest;
public sealed record GetCustomerConsentSummaryQuery(Guid CustomerId) : IRequest<IReadOnlyList<CustomerConsentDto>>;
public sealed record CustomerConsentDto(Guid Id, CustomerConsentChannel Channel, CustomerConsentPurpose Purpose, CustomerConsentStatus Status, CustomerConsentSource Source, DateTime ValidFromUtc, DateTime? ValidUntilUtc, string? EvidenceText);

public sealed record ChangeCustomerLifecycleStageCommand(Guid CustomerId, CustomerLifecycleStage NewStage, string? Reason) : IRequest;
public sealed record RecalculateCustomerDataQualityCommand(Guid CustomerId) : IRequest<int>;
public sealed record GetCustomerDataQualityQuery(Guid CustomerId) : IRequest<int?>;
public sealed record RecalculateCustomerRelationshipHealthCommand(Guid CustomerId) : IRequest<int>;
public sealed record UpsertCustomerStakeholderCommand(Guid CompanyId, Guid ContactId, Guid? RelatedOpportunityId, CustomerStakeholderRole Role, CustomerInfluenceLevel InfluenceLevel, CustomerSentiment Sentiment, string? Notes, bool IsPrimary) : IRequest<Guid>;
public sealed record GetStakeholderMapQuery(Guid CompanyId) : IRequest<IReadOnlyList<CustomerStakeholderDto>>;
public sealed record CustomerStakeholderDto(Guid Id, Guid CompanyId, Guid ContactId, string ContactName, CustomerStakeholderRole Role, CustomerInfluenceLevel InfluenceLevel, CustomerSentiment Sentiment, bool IsPrimary, string? Notes);
public sealed record ShareCustomerRecordCommand(CustomerEntityType EntityType, Guid EntityId, Guid? SharedWithUserId, Guid? SharedWithTeamId, CustomerRecordAccessLevel AccessLevel, DateTime? ValidUntilUtc, string Reason) : IRequest<Guid>;
public sealed record RevokeCustomerRecordShareCommand(Guid ShareId) : IRequest;
public sealed record GetCustomerRecordSharesQuery(CustomerEntityType EntityType, Guid EntityId) : IRequest<IReadOnlyList<CustomerRecordShareDto>>;
public sealed record CustomerRecordShareDto(Guid Id, CustomerEntityType EntityType, Guid EntityId, Guid? SharedWithUserId, Guid? SharedWithTeamId, CustomerRecordAccessLevel AccessLevel, DateTime? ValidUntilUtc, string Reason);
public sealed record GetCustomerAuditTimelineQuery(Guid CustomerId, int Page = 1, int PageSize = 50) : IRequest<IReadOnlyList<CustomerAuditEventDto>>;
public sealed record CustomerAuditEventDto(Guid Id, CustomerAuditAction Action, string? FieldName, string? OldValueMasked, string? NewValueMasked, Guid ActorUserId, DateTime OccurredAtUtc, string CorrelationId);
public sealed record SearchCustomersQuery(string Term, int Take = 20) : IRequest<IReadOnlyList<CustomerSearchResultDto>>;
public sealed record CustomerSearchResultDto(Guid Id, string Title, string? Subtitle, string? Email, string? Phone);
public sealed record ReindexCustomerSearchDocumentCommand(Guid CustomerId) : IRequest<Guid>;
public sealed record UpsertEnrichmentProfileCommand(CustomerEntityType EntityType, Guid EntityId, string? Website, string? Domain, string? LinkedInUrl, string? Industry, int? EmployeeCount, decimal? AnnualRevenue, string? Country, string? City, string? SocialProfilesJson, string? Source, decimal ConfidenceScore, string? RawDataJson) : IRequest<Guid>;
public sealed record FindCustomerDuplicatesQuery(Guid CustomerId) : IRequest<IReadOnlyList<NetMetric.CRM.CustomerManagement.Application.Features.Customer360.CustomerDuplicateWarningDto>>;
public sealed record GetDuplicateMergePreviewQuery(Guid MasterCustomerId, Guid DuplicateCustomerId) : IRequest<CustomerMergePreviewDto>;
public sealed record MergeCustomersCommand(Guid MasterCustomerId, Guid DuplicateCustomerId, IReadOnlyDictionary<string, string?> ResolvedFields, string Reason) : IRequest<Guid>;
public sealed record CustomerMergePreviewDto(Guid MasterCustomerId, Guid DuplicateCustomerId, IReadOnlyList<CustomerMergeConflictDto> Conflicts, IReadOnlyList<string> RelatedRecordsToReassign);
public sealed record CustomerMergeConflictDto(string FieldName, string? MasterValue, string? DuplicateValue, string? RecommendedValue);

public sealed class UpsertCustomerConsentCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerAuditTrailWriter auditWriter) : IRequestHandler<UpsertCustomerConsentCommand, Guid>
{
    public async Task<Guid> Handle(UpsertCustomerConsentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        await EnsureCustomerAsync(dbContext, tenantId, request.CustomerId, cancellationToken);

        var consent = await dbContext.CustomerConsents.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == request.CustomerId && x.Channel == request.Channel && x.Purpose == request.Purpose, cancellationToken);
        var previous = consent?.Status ?? CustomerConsentStatus.Unknown;
        var created = consent is null;
        consent ??= new CustomerConsent { TenantId = tenantId, EntityType = CustomerEntityType.Customer, EntityId = request.CustomerId, Channel = request.Channel, Purpose = request.Purpose, ValidFromUtc = DateTime.UtcNow };
        consent.Status = request.Status;
        consent.Source = request.Source;
        consent.ValidUntilUtc = request.ValidUntilUtc;
        consent.EvidenceText = request.EvidenceText;
        consent.EvidenceIpAddress = request.EvidenceIpAddress;
        consent.EvidenceUserAgent = request.EvidenceUserAgent;
        if (created)
            await dbContext.CustomerConsents.AddAsync(consent, cancellationToken);
        await dbContext.CustomerConsentHistories.AddAsync(new CustomerConsentHistory { TenantId = tenantId, ConsentId = consent.Id, PreviousStatus = previous, NewStatus = request.Status, ChangedAtUtc = DateTime.UtcNow, ChangedBy = currentUserService.UserName ?? currentUserService.UserId.ToString(), Reason = request.Reason, Source = request.Source }, cancellationToken);
        await auditWriter.WriteAsync(CustomerEntityType.Customer, request.CustomerId, CustomerAuditAction.ConsentChanged, "Consent", previous.ToString(), request.Status.ToString(), null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return consent.Id;
    }

    private static async Task EnsureCustomerAsync(ICustomerManagementDbContext dbContext, Guid tenantId, Guid customerId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Customers.AnyAsync(x => x.TenantId == tenantId && x.Id == customerId, cancellationToken))
            throw new NotFoundAppException("Customer not found.");
    }
}

public sealed class RevokeCustomerConsentCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerAuditTrailWriter auditWriter) : IRequestHandler<RevokeCustomerConsentCommand>
{
    public async Task Handle(RevokeCustomerConsentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var consent = await dbContext.CustomerConsents.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EntityId == request.CustomerId && x.Id == request.ConsentId, cancellationToken) ?? throw new NotFoundAppException("Consent not found.");
        var previous = consent.Status;
        consent.Status = CustomerConsentStatus.Revoked;
        await dbContext.CustomerConsentHistories.AddAsync(new CustomerConsentHistory { TenantId = tenantId, ConsentId = consent.Id, PreviousStatus = previous, NewStatus = consent.Status, ChangedAtUtc = DateTime.UtcNow, ChangedBy = currentUserService.UserName ?? currentUserService.UserId.ToString(), Reason = request.Reason, Source = consent.Source }, cancellationToken);
        await auditWriter.WriteAsync(CustomerEntityType.Customer, request.CustomerId, CustomerAuditAction.ConsentChanged, "Consent", previous.ToString(), consent.Status.ToString(), null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetCustomerConsentSummaryQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerManagementSecurityService securityService) : IRequestHandler<GetCustomerConsentSummaryQuery, IReadOnlyList<CustomerConsentDto>>
{
    public async Task<IReadOnlyList<CustomerConsentDto>> Handle(GetCustomerConsentSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        return await dbContext.CustomerConsents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == request.CustomerId)
            .OrderBy(x => x.Channel).ThenBy(x => x.Purpose)
            .Select(x => new CustomerConsentDto(x.Id, x.Channel, x.Purpose, x.Status, x.Source, x.ValidFromUtc, x.ValidUntilUtc, securityService.Mask(nameof(CustomerConsent), nameof(CustomerConsent.EvidenceText), x.EvidenceText)))
            .ToListAsync(cancellationToken);
    }
}

public sealed class ChangeCustomerLifecycleStageCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerAuditTrailWriter auditWriter) : IRequestHandler<ChangeCustomerLifecycleStageCommand>
{
    public async Task Handle(ChangeCustomerLifecycleStageCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        if ((request.NewStage is CustomerLifecycleStage.Churned or CustomerLifecycleStage.Reactivated) && string.IsNullOrWhiteSpace(request.Reason))
            throw new ValidationAppException("Reason is required for critical lifecycle transitions.");
        if (!await dbContext.Customers.AnyAsync(x => x.TenantId == tenantId && x.Id == request.CustomerId, cancellationToken))
            throw new NotFoundAppException("Customer not found.");
        var previous = await dbContext.CustomerLifecycleStageHistories.AsNoTracking().Where(x => x.TenantId == tenantId && x.CustomerId == request.CustomerId).OrderByDescending(x => x.ChangedAtUtc).Select(x => (CustomerLifecycleStage?)x.NewStage).FirstOrDefaultAsync(cancellationToken);
        await dbContext.CustomerLifecycleStageHistories.AddAsync(new CustomerLifecycleStageHistory { TenantId = tenantId, CustomerId = request.CustomerId, PreviousStage = previous, NewStage = request.NewStage, ChangedAtUtc = DateTime.UtcNow, ChangedBy = currentUserService.UserName ?? currentUserService.UserId.ToString(), Reason = request.Reason }, cancellationToken);
        await auditWriter.WriteAsync(CustomerEntityType.Customer, request.CustomerId, CustomerAuditAction.StageChanged, "LifecycleStage", previous?.ToString(), request.NewStage.ToString(), null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class RecalculateCustomerDataQualityCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerDataQualityService dataQualityService, IDuplicateDetectionService duplicateDetectionService) : IRequestHandler<RecalculateCustomerDataQualityCommand, int>
{
    public async Task<int> Handle(RecalculateCustomerDataQualityCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var customer = await dbContext.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.CustomerId, cancellationToken) ?? throw new NotFoundAppException("Customer not found.");
        var consents = await dbContext.CustomerConsents.AsNoTracking().Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id).ToListAsync(cancellationToken);
        var duplicateRisk = (await duplicateDetectionService.FindCustomerDuplicatesAsync(customer, cancellationToken)).FirstOrDefault()?.Score ?? 0;
        var snapshot = dataQualityService.Calculate(customer, consents, duplicateRisk, null);
        var existing = await dbContext.CustomerDataQualitySnapshots.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id, cancellationToken);
        if (existing is null) await dbContext.CustomerDataQualitySnapshots.AddAsync(snapshot, cancellationToken);
        else Copy(snapshot, existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot.Score;
    }

    private static void Copy(CustomerDataQualitySnapshot source, CustomerDataQualitySnapshot target)
    {
        target.Score = source.Score;
        target.MissingFieldsJson = source.MissingFieldsJson;
        target.InvalidFieldsJson = source.InvalidFieldsJson;
        target.DuplicateRiskScore = source.DuplicateRiskScore;
        target.StaleDataRiskScore = source.StaleDataRiskScore;
        target.RecommendationsJson = source.RecommendationsJson;
        target.CalculatedAtUtc = source.CalculatedAtUtc;
    }
}

public sealed class GetCustomerDataQualityQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetCustomerDataQualityQuery, int?>
{
    public async Task<int?> Handle(GetCustomerDataQualityQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        return await dbContext.CustomerDataQualitySnapshots.AsNoTracking().Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == request.CustomerId).OrderByDescending(x => x.CalculatedAtUtc).Select(x => (int?)x.Score).FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class RecalculateCustomerRelationshipHealthCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerRelationshipHealthService healthService) : IRequestHandler<RecalculateCustomerRelationshipHealthCommand, int>
{
    public async Task<int> Handle(RecalculateCustomerRelationshipHealthCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var customer = await dbContext.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.CustomerId, cancellationToken) ?? throw new NotFoundAppException("Customer not found.");
        var lifecycle = await dbContext.CustomerLifecycleStageHistories.AsNoTracking().Where(x => x.TenantId == tenantId && x.CustomerId == customer.Id).OrderByDescending(x => x.ChangedAtUtc).Select(x => (CustomerLifecycleStage?)x.NewStage).FirstOrDefaultAsync(cancellationToken);
        var snapshot = healthService.Calculate(customer, null, 0, 0, 0, 0, 0, null, lifecycle);
        var existing = await dbContext.CustomerRelationshipHealthSnapshots.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.CustomerId == customer.Id, cancellationToken);
        if (existing is null)
        {
            await dbContext.CustomerRelationshipHealthSnapshots.AddAsync(snapshot, cancellationToken);
        }
        else
        {
            existing.Score = snapshot.Score;
            existing.RiskLevel = snapshot.RiskLevel;
            existing.SignalsJson = snapshot.SignalsJson;
            existing.CalculatedAtUtc = snapshot.CalculatedAtUtc;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot.Score;
    }
}

public sealed class UpsertCustomerStakeholderCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpsertCustomerStakeholderCommand, Guid>
{
    public async Task<Guid> Handle(UpsertCustomerStakeholderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var contact = await dbContext.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ContactId, cancellationToken) ?? throw new NotFoundAppException("Contact not found.");
        if (contact.CompanyId != request.CompanyId)
            throw new ValidationAppException("Stakeholder contact must belong to the requested company.");
        if (!await dbContext.Companies.AnyAsync(x => x.TenantId == tenantId && x.Id == request.CompanyId, cancellationToken))
            throw new NotFoundAppException("Company not found.");

        var entity = await dbContext.CustomerStakeholders.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.CompanyId == request.CompanyId && x.ContactId == request.ContactId && x.Role == request.Role, cancellationToken);
        var created = entity is null;
        entity ??= new CustomerStakeholder { TenantId = tenantId, CompanyId = request.CompanyId, ContactId = request.ContactId, Role = request.Role };
        entity.RelatedOpportunityId = request.RelatedOpportunityId;
        entity.InfluenceLevel = request.InfluenceLevel;
        entity.Sentiment = request.Sentiment;
        entity.Notes = request.Notes;
        entity.IsPrimary = request.IsPrimary;
        if (created)
            await dbContext.CustomerStakeholders.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed class GetStakeholderMapQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetStakeholderMapQuery, IReadOnlyList<CustomerStakeholderDto>>
{
    public async Task<IReadOnlyList<CustomerStakeholderDto>> Handle(GetStakeholderMapQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var contacts = dbContext.Contacts.AsNoTracking();
        return await dbContext.CustomerStakeholders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CompanyId == request.CompanyId)
            .Join(contacts, s => s.ContactId, c => c.Id, (s, c) => new CustomerStakeholderDto(s.Id, s.CompanyId, s.ContactId, c.FullName, s.Role, s.InfluenceLevel, s.Sentiment, s.IsPrimary, s.Notes))
            .ToListAsync(cancellationToken);
    }
}

public sealed class ShareCustomerRecordCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerAuditTrailWriter auditWriter) : IRequestHandler<ShareCustomerRecordCommand, Guid>
{
    public async Task<Guid> Handle(ShareCustomerRecordCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        if (!request.SharedWithUserId.HasValue && !request.SharedWithTeamId.HasValue)
            throw new ValidationAppException("SharedWithUserId or SharedWithTeamId is required.");
        if (request.AccessLevel == CustomerRecordAccessLevel.OwnerDelegate && !currentUserService.HasPermission("crm.customer-management.customers.manage"))
            throw new ForbiddenAppException("Owner delegate share requires customer management permission.");
        await EnsureEntityExists(dbContext, tenantId, request.EntityType, request.EntityId, cancellationToken);
        var share = new CustomerRecordShare { TenantId = tenantId, EntityType = request.EntityType, EntityId = request.EntityId, SharedWithUserId = request.SharedWithUserId, SharedWithTeamId = request.SharedWithTeamId, AccessLevel = request.AccessLevel, ValidUntilUtc = request.ValidUntilUtc, Reason = request.Reason };
        await dbContext.CustomerRecordShares.AddAsync(share, cancellationToken);
        await auditWriter.WriteAsync(request.EntityType, request.EntityId, CustomerAuditAction.Shared, null, null, request.AccessLevel.ToString(), null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return share.Id;
    }

    private static Task EnsureEntityExists(ICustomerManagementDbContext dbContext, Guid tenantId, CustomerEntityType type, Guid id, CancellationToken cancellationToken)
        => type switch
        {
            CustomerEntityType.Customer => dbContext.Customers.AnyAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken).ContinueWith(t => { if (!t.Result) throw new NotFoundAppException("Customer not found."); }, cancellationToken),
            CustomerEntityType.Company => dbContext.Companies.AnyAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken).ContinueWith(t => { if (!t.Result) throw new NotFoundAppException("Company not found."); }, cancellationToken),
            CustomerEntityType.Contact => dbContext.Contacts.AnyAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken).ContinueWith(t => { if (!t.Result) throw new NotFoundAppException("Contact not found."); }, cancellationToken),
            _ => Task.CompletedTask
        };
}

public sealed class RevokeCustomerRecordShareCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<RevokeCustomerRecordShareCommand>
{
    public async Task Handle(RevokeCustomerRecordShareCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var share = await dbContext.CustomerRecordShares.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ShareId, cancellationToken) ?? throw new NotFoundAppException("Share not found.");
        share.IsDeleted = true;
        share.DeletedAt = DateTime.UtcNow;
        share.DeletedBy = currentUserService.UserName ?? currentUserService.UserId.ToString();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetCustomerRecordSharesQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetCustomerRecordSharesQuery, IReadOnlyList<CustomerRecordShareDto>>
{
    public async Task<IReadOnlyList<CustomerRecordShareDto>> Handle(GetCustomerRecordSharesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        return await dbContext.CustomerRecordShares.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == request.EntityType && x.EntityId == request.EntityId && (!x.ValidUntilUtc.HasValue || x.ValidUntilUtc.Value >= DateTime.UtcNow))
            .Select(x => new CustomerRecordShareDto(x.Id, x.EntityType, x.EntityId, x.SharedWithUserId, x.SharedWithTeamId, x.AccessLevel, x.ValidUntilUtc, x.Reason))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetCustomerAuditTimelineQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetCustomerAuditTimelineQuery, IReadOnlyList<CustomerAuditEventDto>>
{
    public async Task<IReadOnlyList<CustomerAuditEventDto>> Handle(GetCustomerAuditTimelineQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        return await dbContext.CustomerAuditEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == request.CustomerId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CustomerAuditEventDto(x.Id, x.Action, x.FieldName, x.OldValueMasked, x.NewValueMasked, x.ActorUserId, x.OccurredAtUtc, x.CorrelationId))
            .ToListAsync(cancellationToken);
    }
}

public sealed class SearchCustomersQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerManagementSecurityService securityService) : IRequestHandler<SearchCustomersQuery, IReadOnlyList<CustomerSearchResultDto>>
{
    public async Task<IReadOnlyList<CustomerSearchResultDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var term = request.Term.Trim().ToLowerInvariant();
        var readableCustomerIds = securityService.ApplyCustomerReadScope(dbContext.Customers.AsNoTracking())
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => x.Id);

        var results = await dbContext.CustomerSearchDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && readableCustomerIds.Contains(x.EntityId) && x.SearchText.Contains(term))
            .OrderBy(x => x.Title)
            .Take(Math.Clamp(request.Take, 1, 50))
            .Select(x => new { x.EntityId, x.Title, x.Subtitle, x.NormalizedEmail, x.NormalizedPhone })
            .ToListAsync(cancellationToken);

        return results
            .Select(x => new CustomerSearchResultDto(
                x.EntityId,
                x.Title,
                x.Subtitle,
                securityService.Mask(nameof(Customer), nameof(Customer.Email), x.NormalizedEmail),
                securityService.Mask(nameof(Customer), nameof(Customer.MobilePhone), x.NormalizedPhone)))
            .ToList();
    }
}

public sealed class ReindexCustomerSearchDocumentCommandHandler(ICustomerSearchIndexer indexer) : IRequestHandler<ReindexCustomerSearchDocumentCommand, Guid>
{
    public async Task<Guid> Handle(ReindexCustomerSearchDocumentCommand request, CancellationToken cancellationToken)
        => (await indexer.ReindexCustomerAsync(request.CustomerId, cancellationToken)).Id;
}

public sealed class UpsertEnrichmentProfileCommandHandler(ICurrentUserService currentUserService, ICustomerEnrichmentService enrichmentService, IDuplicateDetectionService duplicateDetectionService) : IRequestHandler<UpsertEnrichmentProfileCommand, Guid>
{
    public async Task<Guid> Handle(UpsertEnrichmentProfileCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        if (!string.IsNullOrWhiteSpace(request.Website) && !Uri.TryCreate(request.Website.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? request.Website : $"https://{request.Website}", UriKind.Absolute, out _))
        {
            throw new ValidationAppException("Website URL is invalid.");
        }
        var profile = new CustomerEnrichmentProfile { TenantId = tenantId, EntityType = request.EntityType, EntityId = request.EntityId, Website = request.Website, Domain = duplicateDetectionService.NormalizeDomain(request.Domain ?? request.Website), LinkedInUrl = request.LinkedInUrl, Industry = request.Industry, EmployeeCount = request.EmployeeCount, AnnualRevenue = request.AnnualRevenue, Country = request.Country, City = request.City, SocialProfilesJson = request.SocialProfilesJson, Source = request.Source ?? "Manual", ConfidenceScore = request.ConfidenceScore, RawDataJson = request.RawDataJson };
        return (await enrichmentService.UpsertManualAsync(profile, cancellationToken)).Id;
    }
}

public sealed class FindCustomerDuplicatesQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, IDuplicateDetectionService duplicateDetectionService) : IRequestHandler<FindCustomerDuplicatesQuery, IReadOnlyList<NetMetric.CRM.CustomerManagement.Application.Features.Customer360.CustomerDuplicateWarningDto>>
{
    public async Task<IReadOnlyList<NetMetric.CRM.CustomerManagement.Application.Features.Customer360.CustomerDuplicateWarningDto>> Handle(FindCustomerDuplicatesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var customer = await dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.CustomerId, cancellationToken) ?? throw new NotFoundAppException("Customer not found.");
        return await duplicateDetectionService.FindCustomerDuplicatesAsync(customer, cancellationToken);
    }
}

public sealed class GetDuplicateMergePreviewQueryHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetDuplicateMergePreviewQuery, CustomerMergePreviewDto>
{
    public async Task<CustomerMergePreviewDto> Handle(GetDuplicateMergePreviewQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var master = await dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.MasterCustomerId, cancellationToken) ?? throw new NotFoundAppException("Master customer not found.");
        var duplicate = await dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.DuplicateCustomerId, cancellationToken) ?? throw new NotFoundAppException("Duplicate customer not found.");
        var conflicts = new List<CustomerMergeConflictDto>();
        AddConflict(nameof(Customer.Email), master.Email, duplicate.Email);
        AddConflict(nameof(Customer.MobilePhone), master.MobilePhone, duplicate.MobilePhone);
        AddConflict(nameof(Customer.WorkPhone), master.WorkPhone, duplicate.WorkPhone);
        AddConflict(nameof(Customer.JobTitle), master.JobTitle, duplicate.JobTitle);
        AddConflict(nameof(Customer.CompanyId), master.CompanyId?.ToString(), duplicate.CompanyId?.ToString());
        return new CustomerMergePreviewDto(master.Id, duplicate.Id, conflicts, ["Contacts", "Addresses", "Documents", "Consents", "Search documents"]);

        void AddConflict(string field, string? left, string? right)
        {
            if (!string.Equals(left, right, StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(left) || !string.IsNullOrWhiteSpace(right)))
                conflicts.Add(new CustomerMergeConflictDto(field, left, right, string.IsNullOrWhiteSpace(left) ? right : left));
        }
    }
}

public sealed class MergeCustomersCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerAuditTrailWriter auditWriter) : IRequestHandler<MergeCustomersCommand, Guid>
{
    public async Task<Guid> Handle(MergeCustomersCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        if (request.MasterCustomerId == request.DuplicateCustomerId)
            throw new ValidationAppException("Master and duplicate customer cannot be the same.");
        var master = await dbContext.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.MasterCustomerId, cancellationToken) ?? throw new NotFoundAppException("Master customer not found.");
        var duplicate = await dbContext.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.DuplicateCustomerId, cancellationToken) ?? throw new NotFoundAppException("Duplicate customer not found.");
        ApplyResolved(master, request.ResolvedFields);
        await dbContext.Contacts.Where(x => x.TenantId == tenantId && x.CustomerId == duplicate.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.CustomerId, master.Id), cancellationToken);
        await dbContext.Set<Address>().Where(x => x.TenantId == tenantId && x.CustomerId == duplicate.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.CustomerId, master.Id), cancellationToken);
        await dbContext.CustomerConsents.Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == duplicate.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.EntityId, master.Id), cancellationToken);
        duplicate.IsDeleted = true;
        duplicate.DeletedAt = DateTime.UtcNow;
        duplicate.DeletedBy = currentUserService.UserName ?? currentUserService.UserId.ToString();
        await auditWriter.WriteAsync(CustomerEntityType.Customer, master.Id, CustomerAuditAction.Merged, null, duplicate.Id.ToString(), master.Id.ToString(), JsonSerializer.Serialize(new { request.Reason }), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return master.Id;
    }

    private static void ApplyResolved(Customer master, IReadOnlyDictionary<string, string?> fields)
    {
        foreach (var field in fields)
        {
            switch (field.Key)
            {
                case nameof(Customer.Email): master.Email = field.Value; break;
                case nameof(Customer.MobilePhone): master.MobilePhone = field.Value; break;
                case nameof(Customer.WorkPhone): master.WorkPhone = field.Value; break;
                case nameof(Customer.JobTitle): master.JobTitle = field.Value; break;
            }
        }
    }
}

public sealed class CustomerConsentCommandValidator : AbstractValidator<UpsertCustomerConsentCommand>
{
    public CustomerConsentCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.EvidenceText).MaximumLength(2000);
        RuleFor(x => x.EvidenceIpAddress).MaximumLength(128);
        RuleFor(x => x.EvidenceUserAgent).MaximumLength(512);
    }
}

public sealed class ShareCustomerRecordCommandValidator : AbstractValidator<ShareCustomerRecordCommand>
{
    public ShareCustomerRecordCommandValidator()
    {
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x).Must(x => x.SharedWithUserId.HasValue || x.SharedWithTeamId.HasValue).WithMessage("SharedWithUserId or SharedWithTeamId is required.");
    }
}
