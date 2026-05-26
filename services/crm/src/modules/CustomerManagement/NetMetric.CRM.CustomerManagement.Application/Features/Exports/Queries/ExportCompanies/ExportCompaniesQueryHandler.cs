// <copyright file="ExportCompaniesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Exports;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Exports.Queries.ExportCompanies;

public sealed class ExportCompaniesQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService)
    : IRequestHandler<ExportCompaniesQuery, ExportFileDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<ExportFileDto> Handle(ExportCompaniesQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;

        var query = _securityService.ApplyCompanyReadScope(_dbContext.Set<Company>())
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Name, search) ||
                (x.Email != null && EF.Functions.Like(x.Email, search)) ||
                (x.Phone != null && EF.Functions.Like(x.Phone, search)) ||
                (x.TaxNumber != null && EF.Functions.Like(x.TaxNumber, search)) ||
                (x.Sector != null && EF.Functions.Like(x.Sector, search)));
        }

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var rows = await query
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Email,
                x.Phone,
                CompanyType = x.CompanyType.ToString(),
                x.Sector,
                x.Website,
                x.OwnerUserId,
                x.ParentCompanyId,
                x.IsActive,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var content = CsvExportBuilder.Build(
            headers: ["Id", "Name", "Email", "Phone", "CompanyType", "Sector", "Website", "OwnerUserId", "ParentCompanyId", "IsActive", "CreatedAtUtc"],
            rows: rows,
            selector: x =>
            [
                x.Id.ToString(),
                x.Name,
                _securityService.Mask(nameof(Company), nameof(Company.Email), x.Email),
                _securityService.Mask(nameof(Company), nameof(Company.Phone), x.Phone),
                x.CompanyType,
                x.Sector,
                x.Website,
                x.OwnerUserId?.ToString(),
                x.ParentCompanyId?.ToString(),
                x.IsActive.ToString(),
                x.CreatedAt.ToString("O")
            ]);

        return new ExportFileDto
        {
            FileName = $"companies-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv",
            Content = content
        };
    }
}
