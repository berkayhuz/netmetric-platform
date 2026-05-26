// <copyright file="CustomerOperationalServices.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CRM.Documents;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services.CustomerOperations;

public sealed class EmptyCustomer360Provider :
    ICustomerLeadSummaryProvider,
    ICustomerOpportunitySummaryProvider,
    ICustomerDealSummaryProvider,
    ICustomerQuoteSummaryProvider,
    ICustomerTicketSummaryProvider,
    ICustomerFinanceSummaryProvider,
    ICustomerContractSummaryProvider,
    ICustomerDocumentSummaryProvider,
    ICustomerActivitySummaryProvider,
    ICustomerCommunicationSummaryProvider
{
    private static readonly IReadOnlyList<Customer360SummaryItemDto> Empty = [];
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenLeadsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenOpportunitiesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenDealsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetQuotesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetTicketsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<CustomerFinanceSummaryDto> GetFinanceSummaryAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(new CustomerFinanceSummaryDto(null, null, 0, 0, [], []));
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetContractsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetDocumentsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetActivitiesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
    public Task<IReadOnlyList<Customer360SummaryItemDto>> GetCommunicationsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken) => Task.FromResult(Empty);
}

public sealed class CustomerManagementDocumentSummaryProvider(ICustomerManagementDbContext dbContext) : ICustomerDocumentSummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetDocumentsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
        => await dbContext.Set<Document>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value)))
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.FileName, x.ContentType, x.CreatedAt, null, x.Url))
            .ToListAsync(cancellationToken);
}

public sealed class DuplicateDetectionService(ICustomerManagementDbContext dbContext) : IDuplicateDetectionService
{
    public string? NormalizeEmail(string? email) => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    public string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length > 10 && digits.StartsWith('0')) digits = digits.TrimStart('0');
        return digits.Length == 0 ? null : digits;
    }

    public string? NormalizeDomain(string? websiteOrDomain) => NormalizeDomainValue(websiteOrDomain);

    internal static string? NormalizeDomainValue(string? websiteOrDomain)
    {
        if (string.IsNullOrWhiteSpace(websiteOrDomain))
        {
            return null;
        }

        var value = websiteOrDomain.Trim().ToLowerInvariant();
        if (Uri.TryCreate(value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? value : $"https://{value}", UriKind.Absolute, out var uri))
        {
            value = uri.Host;
        }

        return value.StartsWith("www.", StringComparison.Ordinal) ? value[4..] : value.Trim('/');
    }

    public decimal CalculateNameSimilarity(string? left, string? right)
    {
        left = NormalizeName(left);
        right = NormalizeName(right);
        if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return 0;
        if (left == right) return 1;
        var distance = Levenshtein(left, right);
        return Math.Round(1m - ((decimal)distance / Math.Max(left.Length, right.Length)), 2);
    }

    public async Task<IReadOnlyList<CustomerDuplicateWarningDto>> FindCustomerDuplicatesAsync(Customer customer, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(customer.Email);
        var phone = NormalizePhone(customer.MobilePhone ?? customer.WorkPhone ?? customer.PersonalPhone);
        var candidates = await dbContext.Customers
            .AsNoTracking()
            .Where(x => x.TenantId == customer.TenantId && x.Id != customer.Id)
            .Take(200)
            .ToListAsync(cancellationToken);

        var warnings = new List<CustomerDuplicateWarningDto>();
        foreach (var candidate in candidates)
        {
            var reasons = new List<string>();
            decimal score = 0;
            if (email is not null && email == NormalizeEmail(candidate.Email))
            {
                score += 60;
                reasons.Add("Exact email match");
            }
            if (phone is not null && phone == NormalizePhone(candidate.MobilePhone ?? candidate.WorkPhone ?? candidate.PersonalPhone))
            {
                score += 30;
                reasons.Add("Normalized phone match");
            }
            if (!string.IsNullOrWhiteSpace(customer.IdentityNumber) && customer.IdentityNumber == candidate.IdentityNumber)
            {
                score += 80;
                reasons.Add("Identity number match");
            }
            var nameSimilarity = CalculateNameSimilarity(customer.FullName, candidate.FullName);
            if (nameSimilarity >= 0.85m)
            {
                score += Math.Round(nameSimilarity * 20, 2);
                reasons.Add("Customer name similarity");
            }

            if (score >= 50)
                warnings.Add(new CustomerDuplicateWarningDto(candidate.Id, CustomerEntityType.Customer, Math.Min(100, score), reasons));
        }

        return warnings.OrderByDescending(x => x.Score).Take(10).ToList();
    }

    private static string NormalizeName(string? value) => new((value ?? string.Empty).Trim().ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());

    private static int Levenshtein(string left, string right)
    {
        var matrix = new int[left.Length + 1, right.Length + 1];
        for (var i = 0; i <= left.Length; i++) matrix[i, 0] = i;
        for (var j = 0; j <= right.Length; j++) matrix[0, j] = j;
        for (var i = 1; i <= left.Length; i++)
        {
            for (var j = 1; j <= right.Length; j++)
            {
                var cost = left[i - 1] == right[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[left.Length, right.Length];
    }
}

public sealed class CustomerDataQualityService : ICustomerDataQualityService
{
    public CustomerDataQualitySnapshot Calculate(Customer customer, IReadOnlyCollection<CustomerConsent> consents, decimal duplicateRiskScore, DateTime? lastActivityAtUtc)
    {
        var missing = new List<string>();
        var invalid = new List<string>();
        var recommendations = new List<string>();
        var score = 100;

        PenalizeMissing(customer.FirstName, "FirstName", 10);
        PenalizeMissing(customer.LastName, "LastName", 10);
        if (string.IsNullOrWhiteSpace(customer.Email) && string.IsNullOrWhiteSpace(customer.MobilePhone))
        {
            score -= 20;
            missing.Add("EmailOrPhone");
        }
        if (!string.IsNullOrWhiteSpace(customer.Email) && !customer.Email.Contains('@', StringComparison.Ordinal))
        {
            score -= 15;
            invalid.Add("Email");
        }
        if (!string.IsNullOrWhiteSpace(customer.MobilePhone) && customer.MobilePhone.Count(char.IsDigit) < 7)
        {
            score -= 10;
            invalid.Add("MobilePhone");
        }
        if (!customer.OwnerUserId.HasValue)
        {
            score -= 10;
            missing.Add("OwnerUserId");
        }
        if (consents.Count == 0)
        {
            score -= 10;
            missing.Add("Consent");
        }
        if (lastActivityAtUtc.HasValue && lastActivityAtUtc.Value < DateTime.UtcNow.AddDays(-120))
        {
            score -= 10;
            recommendations.Add("Schedule recent customer activity");
        }
        if (duplicateRiskScore > 0)
        {
            score -= (int)Math.Min(25, duplicateRiskScore / 4);
            recommendations.Add("Review duplicate warning");
        }

        return new CustomerDataQualitySnapshot
        {
            TenantId = customer.TenantId,
            EntityType = CustomerEntityType.Customer,
            EntityId = customer.Id,
            Score = Math.Clamp(score, 0, 100),
            MissingFieldsJson = JsonSerializer.Serialize(missing),
            InvalidFieldsJson = JsonSerializer.Serialize(invalid),
            DuplicateRiskScore = duplicateRiskScore,
            StaleDataRiskScore = lastActivityAtUtc.HasValue && lastActivityAtUtc.Value < DateTime.UtcNow.AddDays(-120) ? 50 : 0,
            RecommendationsJson = JsonSerializer.Serialize(recommendations),
            CalculatedAtUtc = DateTime.UtcNow
        };

        void PenalizeMissing(string? value, string field, int penalty)
        {
            if (!string.IsNullOrWhiteSpace(value)) return;
            score -= penalty;
            missing.Add(field);
        }
    }
}

public sealed class CustomerRelationshipHealthService : ICustomerRelationshipHealthService
{
    public CustomerRelationshipHealthSnapshot Calculate(Customer customer, DateTime? lastActivityAtUtc, int openTickets, int overdueTickets, int openOpportunities, int wonDeals, int unpaidInvoices, int? renewalDueInDays, CustomerLifecycleStage? lifecycleStage)
    {
        var score = 75 + Math.Min(15, openOpportunities * 3) + Math.Min(10, wonDeals * 2);
        if (!lastActivityAtUtc.HasValue || lastActivityAtUtc.Value < DateTime.UtcNow.AddDays(-90)) score -= 20;
        score -= Math.Min(25, openTickets * 5);
        score -= Math.Min(30, overdueTickets * 10);
        score -= Math.Min(25, unpaidInvoices * 8);
        if (renewalDueInDays is >= 0 and <= 30 && (!lastActivityAtUtc.HasValue || lastActivityAtUtc.Value < DateTime.UtcNow.AddDays(-30))) score -= 15;
        if (lifecycleStage == CustomerLifecycleStage.ChurnRisk) score -= 20;
        if (lifecycleStage == CustomerLifecycleStage.Churned) score -= 40;
        score = Math.Clamp(score, 0, 100);

        return new CustomerRelationshipHealthSnapshot
        {
            TenantId = customer.TenantId,
            CustomerId = customer.Id,
            Score = score,
            LastActivityAtUtc = lastActivityAtUtc,
            OpenTicketCount = openTickets,
            OverdueTicketCount = overdueTickets,
            OpenOpportunityCount = openOpportunities,
            WonDealCount = wonDeals,
            UnpaidInvoiceCount = unpaidInvoices,
            RenewalDueInDays = renewalDueInDays,
            RiskLevel = score < 35 ? CustomerRelationshipRiskLevel.Critical : score < 55 ? CustomerRelationshipRiskLevel.High : score < 75 ? CustomerRelationshipRiskLevel.Medium : CustomerRelationshipRiskLevel.Low,
            SignalsJson = JsonSerializer.Serialize(new { openTickets, overdueTickets, openOpportunities, wonDeals, unpaidInvoices, renewalDueInDays, lifecycleStage }),
            CalculatedAtUtc = DateTime.UtcNow
        };
    }
}

public sealed class CustomerAuditTrailWriter(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService, ICustomerManagementSecurityService securityService) : ICustomerAuditTrailWriter
{
    public async Task WriteAsync(CustomerEntityType entityType, Guid entityId, CustomerAuditAction action, string? fieldName, string? oldValue, string? newValue, string? metadataJson, CancellationToken cancellationToken)
    {
        var entityName = entityType.ToString();
        await dbContext.CustomerAuditEvents.AddAsync(new CustomerAuditEvent
        {
            TenantId = currentUserService.EnsureTenant(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            FieldName = fieldName,
            OldValueMasked = fieldName is null ? oldValue : securityService.Mask(entityName, fieldName, oldValue),
            NewValueMasked = fieldName is null ? newValue : securityService.Mask(entityName, fieldName, newValue),
            ActorUserId = currentUserService.EnsureAuthenticated(),
            OccurredAtUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString("N"),
            MetadataJson = metadataJson
        }, cancellationToken);
    }
}

public sealed class CustomerOwnershipRuleEvaluator(ICustomerManagementDbContext dbContext) : ICustomerOwnershipRuleEvaluator
{
    public async Task<CustomerOwnershipRule?> EvaluateAsync(Customer customer, Company? company, CancellationToken cancellationToken)
    {
        var rules = await dbContext.CustomerOwnershipRules
            .AsNoTracking()
            .Where(x => x.TenantId == customer.TenantId && x.IsActive)
            .OrderBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        foreach (var rule in rules)
        {
            var condition = rule.ConditionJson.ToLowerInvariant();
            if ((company?.Sector is not null && condition.Contains(company.Sector.ToLowerInvariant(), StringComparison.Ordinal)) ||
                (company?.Website is not null && condition.Contains(DuplicateDetectionService.NormalizeDomainValue(company.Website) ?? string.Empty, StringComparison.Ordinal)) ||
                condition is "{}" or "")
            {
                return rule;
            }
        }

        return null;
    }
}

public sealed class CustomerSearchIndexer(ICustomerManagementDbContext dbContext, IDuplicateDetectionService duplicateDetectionService) : ICustomerSearchIndexer
{
    public async Task<CustomerSearchDocument> ReindexCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.Include(x => x.Company).FirstAsync(x => x.Id == customerId, cancellationToken);
        var existing = await dbContext.CustomerSearchDocuments.FirstOrDefaultAsync(x => x.TenantId == customer.TenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id, cancellationToken);
        var created = existing is null;
        existing ??= new CustomerSearchDocument { TenantId = customer.TenantId, EntityType = CustomerEntityType.Customer, EntityId = customer.Id };
        existing.Title = customer.FullName;
        existing.Subtitle = customer.Company?.Name;
        existing.NormalizedEmail = duplicateDetectionService.NormalizeEmail(customer.Email);
        existing.NormalizedPhone = duplicateDetectionService.NormalizePhone(customer.MobilePhone ?? customer.WorkPhone ?? customer.PersonalPhone);
        existing.CompanyName = customer.Company?.Name;
        existing.Domain = duplicateDetectionService.NormalizeDomain(customer.Company?.Website);
        existing.SearchText = string.Join(' ', new[] { customer.FullName, customer.Email, customer.MobilePhone, customer.Company?.Name, customer.Company?.Website }.Where(x => !string.IsNullOrWhiteSpace(x))).ToLowerInvariant();
        existing.LastIndexedAtUtc = DateTime.UtcNow;
        if (created)
        {
            await dbContext.CustomerSearchDocuments.AddAsync(existing, cancellationToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }
}

public sealed class CustomerSearchService(ICustomerManagementDbContext dbContext) : ICustomerSearchService
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> SearchAsync(string term, int take, CancellationToken cancellationToken)
    {
        var normalized = term.Trim().ToLowerInvariant();
        return await dbContext.CustomerSearchDocuments
            .AsNoTracking()
            .Where(x => x.SearchText.Contains(normalized))
            .OrderByDescending(x => x.LastIndexedAtUtc)
            .Take(Math.Clamp(take, 1, 50))
            .Select(x => new Customer360SummaryItemDto(x.EntityId, x.Title, x.EntityType.ToString(), x.LastIndexedAtUtc, null, null))
            .ToListAsync(cancellationToken);
    }
}

public sealed class NoExternalCallCustomerEnrichmentProvider : ICustomerEnrichmentProvider
{
    public Task<CustomerEnrichmentProfile?> EnrichAsync(CustomerEntityType entityType, Guid entityId, CancellationToken cancellationToken) => Task.FromResult<CustomerEnrichmentProfile?>(null);
}

public sealed class CustomerEnrichmentService(ICustomerManagementDbContext dbContext) : ICustomerEnrichmentService
{
    public async Task<CustomerEnrichmentProfile> UpsertManualAsync(CustomerEnrichmentProfile profile, CancellationToken cancellationToken)
    {
        var existing = await dbContext.CustomerEnrichmentProfiles.FirstOrDefaultAsync(x => x.TenantId == profile.TenantId && x.EntityType == profile.EntityType && x.EntityId == profile.EntityId, cancellationToken);
        if (existing is null)
        {
            profile.LastEnrichedAtUtc = DateTime.UtcNow;
            await dbContext.CustomerEnrichmentProfiles.AddAsync(profile, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return profile;
        }

        existing.Website = profile.Website;
        existing.Domain = profile.Domain;
        existing.LinkedInUrl = profile.LinkedInUrl;
        existing.Industry = profile.Industry;
        existing.EmployeeCount = profile.EmployeeCount;
        existing.AnnualRevenue = profile.AnnualRevenue;
        existing.Country = profile.Country;
        existing.City = profile.City;
        existing.SocialProfilesJson = profile.SocialProfilesJson;
        existing.Source = profile.Source;
        existing.ConfidenceScore = profile.ConfidenceScore;
        existing.RawDataJson = profile.RawDataJson;
        existing.LastEnrichedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
