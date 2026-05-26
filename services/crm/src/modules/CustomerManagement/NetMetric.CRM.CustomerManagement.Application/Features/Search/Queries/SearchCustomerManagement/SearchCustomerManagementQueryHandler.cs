// <copyright file="SearchCustomerManagementQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Search;
using NetMetric.CurrentUser;
using NetMetric.Pagination;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Search.Queries.SearchCustomerManagement;

public sealed class SearchCustomerManagementQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService) : IRequestHandler<SearchCustomerManagementQuery, PagedResult<CustomerManagementSearchItemDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<PagedResult<CustomerManagementSearchItemDto>> Handle(
        SearchCustomerManagementQuery request,
        CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;
        var term = request.Term?.Trim();
        var results = new List<CustomerManagementSearchItemDto>();
        var candidateLimit = Math.Clamp(request.PageSize * 4, 20, 200);

        if (request.IncludeCompanies)
        {
            var companyQuery = _securityService.ApplyCompanyReadScope(_dbContext.Set<Company>())
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted);

            companyQuery = ApplyCommonCompanyFilters(companyQuery, request, term);

            var companies = await companyQuery
                .OrderBy(x => x.Name)
                .Take(candidateLimit)
                .Select(x => new CustomerManagementSearchItemDto
                {
                    EntityType = "company",
                    Id = x.Id,
                    DisplayName = x.Name,
                    Subtitle = x.Sector,
                    Email = x.Email,
                    Phone = x.Phone,
                    OwnerUserId = x.OwnerUserId,
                    IsActive = x.IsActive,
                    Score = CalculateCompanyScore(x, term)
                })
                .ToListAsync(cancellationToken);

            results.AddRange(companies);
        }

        if (request.IncludeContacts)
        {
            var contactQuery = _securityService.ApplyContactReadScope(_dbContext.Set<Contact>())
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .Include(x => x.Company)
                .Include(x => x.Customer)
                .AsQueryable();

            contactQuery = ApplyCommonContactFilters(contactQuery, request, term);

            var contacts = await contactQuery
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Take(candidateLimit)
                .Select(x => new CustomerManagementSearchItemDto
                {
                    EntityType = "contact",
                    Id = x.Id,
                    DisplayName = (x.FirstName + " " + x.LastName).Trim(),
                    Subtitle = x.Company != null ? x.Company.Name : x.Customer != null ? x.Customer.FirstName + " " + x.Customer.LastName : x.JobTitle,
                    Email = x.Email,
                    Phone = x.MobilePhone ?? x.WorkPhone ?? x.PersonalPhone,
                    OwnerUserId = x.OwnerUserId,
                    IsActive = x.IsActive,
                    Score = CalculateContactScore(x, term)
                })
                .ToListAsync(cancellationToken);

            results.AddRange(contacts);
        }

        if (request.IncludeCustomers)
        {
            var customerQuery = _securityService.ApplyCustomerReadScope(_dbContext.Set<Customer>())
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .Include(x => x.Company)
                .AsQueryable();

            customerQuery = ApplyCommonCustomerFilters(customerQuery, request, term);

            var customers = await customerQuery
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Take(candidateLimit)
                .Select(x => new CustomerManagementSearchItemDto
                {
                    EntityType = "customer",
                    Id = x.Id,
                    DisplayName = (x.FirstName + " " + x.LastName).Trim(),
                    Subtitle = x.Company != null ? x.Company.Name : (x.IsVip ? "VIP" : null),
                    Email = x.Email,
                    Phone = x.MobilePhone ?? x.WorkPhone ?? x.PersonalPhone,
                    OwnerUserId = x.OwnerUserId,
                    IsActive = x.IsActive,
                    Score = CalculateCustomerScore(x, term)
                })
                .ToListAsync(cancellationToken);

            results.AddRange(customers);
        }

        results = ApplyOrdering(results, request);
        var totalCount = results.Count;
        var items = results
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new CustomerManagementSearchItemDto
            {
                EntityType = x.EntityType,
                Id = x.Id,
                DisplayName = x.DisplayName,
                Subtitle = x.Subtitle,
                Email = _securityService.Mask(MapEntityName(x.EntityType), "Email", x.Email),
                Phone = _securityService.Mask(MapEntityName(x.EntityType), "Phone", x.Phone),
                OwnerUserId = x.OwnerUserId,
                IsActive = x.IsActive,
                Score = x.Score
            })
            .ToList();

        return new PagedResult<CustomerManagementSearchItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static IQueryable<Company> ApplyCommonCompanyFilters(
        IQueryable<Company> query,
        SearchCustomerManagementQuery request,
        string? term)
    {
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.OwnerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);

        if (string.IsNullOrWhiteSpace(term))
            return query;

        term = term.Trim();
        return query.Where(x =>
            EF.Functions.Like(x.Name, $"%{term}%") ||
            (x.Email != null && EF.Functions.Like(x.Email, $"%{term}%")) ||
            (x.Phone != null && EF.Functions.Like(x.Phone, $"%{term}%")) ||
            (x.Website != null && EF.Functions.Like(x.Website, $"%{term}%")) ||
            (x.TaxNumber != null && EF.Functions.Like(x.TaxNumber, $"%{term}%")));
    }

    private static IQueryable<Contact> ApplyCommonContactFilters(
        IQueryable<Contact> query,
        SearchCustomerManagementQuery request,
        string? term)
    {
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.OwnerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);

        if (string.IsNullOrWhiteSpace(term))
            return query;

        term = term.Trim();
        return query.Where(x =>
            EF.Functions.Like(x.FirstName, $"%{term}%") ||
            EF.Functions.Like(x.LastName, $"%{term}%") ||
            (x.Email != null && EF.Functions.Like(x.Email, $"%{term}%")) ||
            (x.MobilePhone != null && EF.Functions.Like(x.MobilePhone, $"%{term}%")) ||
            (x.WorkPhone != null && EF.Functions.Like(x.WorkPhone, $"%{term}%")) ||
            (x.JobTitle != null && EF.Functions.Like(x.JobTitle, $"%{term}%")));
    }

    private static IQueryable<Customer> ApplyCommonCustomerFilters(
        IQueryable<Customer> query,
        SearchCustomerManagementQuery request,
        string? term)
    {
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.OwnerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);

        if (string.IsNullOrWhiteSpace(term))
            return query;

        term = term.Trim();
        return query.Where(x =>
            EF.Functions.Like(x.FirstName, $"%{term}%") ||
            EF.Functions.Like(x.LastName, $"%{term}%") ||
            (x.Email != null && EF.Functions.Like(x.Email, $"%{term}%")) ||
            (x.MobilePhone != null && EF.Functions.Like(x.MobilePhone, $"%{term}%")) ||
            (x.WorkPhone != null && EF.Functions.Like(x.WorkPhone, $"%{term}%")) ||
            (x.IdentityNumber != null && EF.Functions.Like(x.IdentityNumber, $"%{term}%")));
    }

    private static List<CustomerManagementSearchItemDto> ApplyOrdering(
        List<CustomerManagementSearchItemDto> items,
        SearchCustomerManagementQuery request)
    {
        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var sortBy = request.SortBy?.Trim().ToLowerInvariant();

        IOrderedEnumerable<CustomerManagementSearchItemDto> ordered = sortBy switch
        {
            "entitytype" => descending ? items.OrderByDescending(x => x.EntityType) : items.OrderBy(x => x.EntityType),
            "email" => descending ? items.OrderByDescending(x => x.Email) : items.OrderBy(x => x.Email),
            "score" => descending ? items.OrderByDescending(x => x.Score) : items.OrderBy(x => x.Score),
            _ => items.OrderByDescending(x => x.Score).ThenBy(x => x.DisplayName)
        };

        return ordered.ToList();
    }

    private static decimal CalculateCompanyScore(Company company, string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return 0;

        term = term.Trim();
        if (string.Equals(company.Name, term, StringComparison.OrdinalIgnoreCase))
            return 100;
        if (company.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            return 80;
        if (!string.IsNullOrWhiteSpace(company.Email) && company.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
            return 70;
        return 50;
    }

    private static decimal CalculateContactScore(Contact contact, string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return 0;

        term = term.Trim();
        var fullName = $"{contact.FirstName} {contact.LastName}".Trim();
        if (string.Equals(fullName, term, StringComparison.OrdinalIgnoreCase))
            return 100;
        if (fullName.Contains(term, StringComparison.OrdinalIgnoreCase))
            return 80;
        if (!string.IsNullOrWhiteSpace(contact.Email) && contact.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
            return 70;
        return 50;
    }

    private static decimal CalculateCustomerScore(Customer customer, string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return 0;

        term = term.Trim();
        var fullName = $"{customer.FirstName} {customer.LastName}".Trim();
        if (string.Equals(fullName, term, StringComparison.OrdinalIgnoreCase))
            return 100;
        if (fullName.Contains(term, StringComparison.OrdinalIgnoreCase))
            return customer.IsVip ? 90 : 80;
        if (!string.IsNullOrWhiteSpace(customer.Email) && customer.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
            return customer.IsVip ? 80 : 70;
        return customer.IsVip ? 60 : 50;
    }

    private static string MapEntityName(string entityType)
        => entityType switch
        {
            "company" => nameof(Company),
            "contact" => nameof(Contact),
            _ => nameof(Customer)
        };
}
