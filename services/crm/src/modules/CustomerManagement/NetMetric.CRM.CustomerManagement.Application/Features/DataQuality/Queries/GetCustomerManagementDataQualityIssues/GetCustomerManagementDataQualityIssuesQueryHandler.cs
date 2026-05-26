// <copyright file="GetCustomerManagementDataQualityIssuesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.DataQuality;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.DataQuality.Queries.GetCustomerManagementDataQualityIssues;

public sealed class GetCustomerManagementDataQualityIssuesQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<GetCustomerManagementDataQualityIssuesQuery, IReadOnlyList<DataQualityIssueDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<IReadOnlyList<DataQualityIssueDto>> Handle(GetCustomerManagementDataQualityIssuesQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;
        var issues = new List<DataQualityIssueDto>();

        var companies = await _dbContext.Set<Company>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Take(request.Take)
            .Select(x => new { x.Id, x.Name, x.Email, x.Phone, x.OwnerUserId })
            .ToListAsync(cancellationToken);

        issues.AddRange(companies.Where(x => string.IsNullOrWhiteSpace(x.Email) && string.IsNullOrWhiteSpace(x.Phone))
            .Select(x => new DataQualityIssueDto
            {
                EntityType = "company",
                EntityId = x.Id,
                DisplayName = x.Name,
                IssueCode = "missing_contact_channels",
                Message = "Company has neither email nor phone.",
                Severity = 2
            }));

        issues.AddRange(companies.Where(x => x.OwnerUserId is null)
            .Select(x => new DataQualityIssueDto
            {
                EntityType = "company",
                EntityId = x.Id,
                DisplayName = x.Name,
                IssueCode = "missing_owner",
                Message = "Company has no owner assigned.",
                Severity = 1
            }));

        var contacts = await _dbContext.Set<Contact>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Take(request.Take)
            .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email, x.MobilePhone, x.CompanyId })
            .ToListAsync(cancellationToken);

        issues.AddRange(contacts.Where(x => string.IsNullOrWhiteSpace(x.Email) && string.IsNullOrWhiteSpace(x.MobilePhone))
            .Select(x => new DataQualityIssueDto
            {
                EntityType = "contact",
                EntityId = x.Id,
                DisplayName = $"{x.FirstName} {x.LastName}".Trim(),
                IssueCode = "missing_contact_channels",
                Message = "Contact has neither email nor mobile phone.",
                Severity = 2
            }));

        issues.AddRange(contacts.Where(x => x.CompanyId is null)
            .Select(x => new DataQualityIssueDto
            {
                EntityType = "contact",
                EntityId = x.Id,
                DisplayName = $"{x.FirstName} {x.LastName}".Trim(),
                IssueCode = "missing_company_link",
                Message = "Contact is not linked to a company.",
                Severity = 1
            }));

        var customers = await _dbContext.Set<Customer>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Take(request.Take)
            .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email, x.MobilePhone, x.CompanyId, x.IdentityNumber, x.IsVip })
            .ToListAsync(cancellationToken);

        issues.AddRange(customers.Where(x => x.IsVip && string.IsNullOrWhiteSpace(x.Email) && string.IsNullOrWhiteSpace(x.MobilePhone))
            .Select(x => new DataQualityIssueDto
            {
                EntityType = "customer",
                EntityId = x.Id,
                DisplayName = $"{x.FirstName} {x.LastName}".Trim(),
                IssueCode = "vip_missing_contact_channels",
                Message = "VIP customer has neither email nor mobile phone.",
                Severity = 3
            }));

        issues.AddRange(customers.Where(x => x.CompanyId is null && string.IsNullOrWhiteSpace(x.IdentityNumber))
            .Select(x => new DataQualityIssueDto
            {
                EntityType = "customer",
                EntityId = x.Id,
                DisplayName = $"{x.FirstName} {x.LastName}".Trim(),
                IssueCode = "weak_identity",
                Message = "Customer has no company link and no identity number.",
                Severity = 2
            }));

        return issues
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.EntityType)
            .Take(request.Take)
            .ToList();
    }
}
