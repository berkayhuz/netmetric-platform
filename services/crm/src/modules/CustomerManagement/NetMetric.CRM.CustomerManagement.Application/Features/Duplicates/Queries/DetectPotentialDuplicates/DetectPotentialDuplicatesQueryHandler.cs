// <copyright file="DetectPotentialDuplicatesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Duplicates;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Queries.DetectPotentialDuplicates;

public sealed class DetectPotentialDuplicatesQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<DetectPotentialDuplicatesQuery, IReadOnlyList<DuplicateCandidateDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<IReadOnlyList<DuplicateCandidateDto>> Handle(DetectPotentialDuplicatesQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;
        var entityType = request.EntityType.Trim().ToLowerInvariant();

        return entityType switch
        {
            EntityNames.Company => await DetectCompanyDuplicatesAsync(tenantId, request, cancellationToken),
            EntityNames.Contact => await DetectContactDuplicatesAsync(tenantId, request, cancellationToken),
            EntityNames.Customer => await DetectCustomerDuplicatesAsync(tenantId, request, cancellationToken),
            _ => []
        };
    }

    private async Task<IReadOnlyList<DuplicateCandidateDto>> DetectCompanyDuplicatesAsync(Guid tenantId, DetectPotentialDuplicatesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _dbContext.Set<Company>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Name, x.Email, x.TaxNumber })
            .ToListAsync(cancellationToken);

        var results = new List<DuplicateCandidateDto>();
        for (var i = 0; i < companies.Count; i++)
        {
            for (var j = i + 1; j < companies.Count; j++)
            {
                var match = EvaluateCompany(companies[i].Name, companies[j].Name, companies[i].Email, companies[j].Email, companies[i].TaxNumber, companies[j].TaxNumber, request.ExactOnly);
                if (match is null)
                    continue;

                results.Add(new DuplicateCandidateDto
                {
                    EntityType = EntityNames.Company,
                    PrimaryId = companies[i].Id,
                    CandidateId = companies[j].Id,
                    PrimaryDisplayName = companies[i].Name,
                    CandidateDisplayName = companies[j].Name,
                    Reason = match.Value.reason,
                    Score = match.Value.score
                });
            }
        }

        return FilterByTerm(results, request.Term);
    }

    private async Task<IReadOnlyList<DuplicateCandidateDto>> DetectContactDuplicatesAsync(Guid tenantId, DetectPotentialDuplicatesQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _dbContext.Set<Contact>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email, x.MobilePhone })
            .ToListAsync(cancellationToken);

        var results = new List<DuplicateCandidateDto>();
        for (var i = 0; i < contacts.Count; i++)
        {
            for (var j = i + 1; j < contacts.Count; j++)
            {
                var fullNameA = $"{contacts[i].FirstName} {contacts[i].LastName}".Trim();
                var fullNameB = $"{contacts[j].FirstName} {contacts[j].LastName}".Trim();
                var match = EvaluatePerson(fullNameA, fullNameB, contacts[i].Email, contacts[j].Email, contacts[i].MobilePhone, contacts[j].MobilePhone, request.ExactOnly);
                if (match is null)
                    continue;

                results.Add(new DuplicateCandidateDto
                {
                    EntityType = EntityNames.Contact,
                    PrimaryId = contacts[i].Id,
                    CandidateId = contacts[j].Id,
                    PrimaryDisplayName = fullNameA,
                    CandidateDisplayName = fullNameB,
                    Reason = match.Value.reason,
                    Score = match.Value.score
                });
            }
        }

        return FilterByTerm(results, request.Term);
    }

    private async Task<IReadOnlyList<DuplicateCandidateDto>> DetectCustomerDuplicatesAsync(Guid tenantId, DetectPotentialDuplicatesQuery request, CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Set<Customer>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email, x.MobilePhone, x.IdentityNumber })
            .ToListAsync(cancellationToken);

        var results = new List<DuplicateCandidateDto>();
        for (var i = 0; i < customers.Count; i++)
        {
            for (var j = i + 1; j < customers.Count; j++)
            {
                var fullNameA = $"{customers[i].FirstName} {customers[i].LastName}".Trim();
                var fullNameB = $"{customers[j].FirstName} {customers[j].LastName}".Trim();
                var match = EvaluateCustomer(fullNameA, fullNameB, customers[i].Email, customers[j].Email, customers[i].MobilePhone, customers[j].MobilePhone, customers[i].IdentityNumber, customers[j].IdentityNumber, request.ExactOnly);
                if (match is null)
                    continue;

                results.Add(new DuplicateCandidateDto
                {
                    EntityType = EntityNames.Customer,
                    PrimaryId = customers[i].Id,
                    CandidateId = customers[j].Id,
                    PrimaryDisplayName = fullNameA,
                    CandidateDisplayName = fullNameB,
                    Reason = match.Value.reason,
                    Score = match.Value.score
                });
            }
        }

        return FilterByTerm(results, request.Term);
    }

    private static IReadOnlyList<DuplicateCandidateDto> FilterByTerm(IEnumerable<DuplicateCandidateDto> results, string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return results.OrderByDescending(x => x.Score).ToList();

        term = term.Trim();
        return results
            .Where(x => x.PrimaryDisplayName.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || x.CandidateDisplayName.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Score)
            .ToList();
    }

    private static (string reason, decimal score)? EvaluateCompany(string nameA, string nameB, string? emailA, string? emailB, string? taxA, string? taxB, bool exactOnly)
    {
        if (!string.IsNullOrWhiteSpace(taxA) && string.Equals(taxA, taxB, StringComparison.OrdinalIgnoreCase))
            return ("Matching tax number", 100);
        if (!string.IsNullOrWhiteSpace(emailA) && string.Equals(emailA, emailB, StringComparison.OrdinalIgnoreCase))
            return ("Matching company email", 95);
        if (string.Equals(nameA, nameB, StringComparison.OrdinalIgnoreCase))
            return ("Matching company name", 90);
        if (!exactOnly && Normalize(nameA) == Normalize(nameB))
            return ("Normalized company name match", 80);
        return null;
    }

    private static (string reason, decimal score)? EvaluatePerson(string fullNameA, string fullNameB, string? emailA, string? emailB, string? phoneA, string? phoneB, bool exactOnly)
    {
        if (!string.IsNullOrWhiteSpace(emailA) && string.Equals(emailA, emailB, StringComparison.OrdinalIgnoreCase))
            return ("Matching email", 100);
        if (!string.IsNullOrWhiteSpace(phoneA) && NormalizePhone(phoneA) == NormalizePhone(phoneB))
            return ("Matching mobile phone", 95);
        if (string.Equals(fullNameA, fullNameB, StringComparison.OrdinalIgnoreCase))
            return ("Matching full name", 85);
        if (!exactOnly && Normalize(fullNameA) == Normalize(fullNameB))
            return ("Normalized name match", 75);
        return null;
    }

    private static (string reason, decimal score)? EvaluateCustomer(string fullNameA, string fullNameB, string? emailA, string? emailB, string? phoneA, string? phoneB, string? identityA, string? identityB, bool exactOnly)
    {
        if (!string.IsNullOrWhiteSpace(identityA) && string.Equals(identityA, identityB, StringComparison.OrdinalIgnoreCase))
            return ("Matching identity number", 100);
        return EvaluatePerson(fullNameA, fullNameB, emailA, emailB, phoneA, phoneB, exactOnly);
    }

    private static string Normalize(string value)
        => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static string NormalizePhone(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new(value.Where(char.IsDigit).ToArray());
}
