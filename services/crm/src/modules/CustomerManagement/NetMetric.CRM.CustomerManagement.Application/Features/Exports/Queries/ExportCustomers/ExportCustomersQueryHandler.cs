// <copyright file="ExportCustomersQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.CustomerManagement.Application.Features.Exports.Queries.ExportCustomers;

public sealed class ExportCustomersQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService)
    : IRequestHandler<ExportCustomersQuery, ExportFileDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<ExportFileDto> Handle(ExportCustomersQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;

        var query = _securityService.ApplyCustomerReadScope(_dbContext.Set<Customer>())
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.FirstName, search) ||
                EF.Functions.Like(x.LastName, search) ||
                (x.Email != null && EF.Functions.Like(x.Email, search)) ||
                (x.MobilePhone != null && EF.Functions.Like(x.MobilePhone, search)) ||
                (x.IdentityNumber != null && EF.Functions.Like(x.IdentityNumber, search)));
        }

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.IsVip.HasValue)
            query = query.Where(x => x.IsVip == request.IsVip.Value);

        var rows = await query
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.MobilePhone,
                CustomerType = x.CustomerType.ToString(),
                x.IdentityNumber,
                x.IsVip,
                x.OwnerUserId,
                x.CompanyId,
                x.IsActive,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var content = CsvExportBuilder.Build(
            headers: ["Id", "FirstName", "LastName", "Email", "MobilePhone", "CustomerType", "IdentityNumber", "IsVip", "OwnerUserId", "CompanyId", "IsActive", "CreatedAtUtc"],
            rows: rows,
            selector: x =>
            [
                x.Id.ToString(),
                x.FirstName,
                x.LastName,
                _securityService.Mask(nameof(Customer), nameof(Customer.Email), x.Email),
                _securityService.Mask(nameof(Customer), nameof(Customer.MobilePhone), x.MobilePhone),
                x.CustomerType,
                _securityService.Mask(nameof(Customer), nameof(Customer.IdentityNumber), x.IdentityNumber),
                x.IsVip.ToString(),
                x.OwnerUserId?.ToString(),
                x.CompanyId?.ToString(),
                x.IsActive.ToString(),
                x.CreatedAt.ToString("O")
            ]);

        return new ExportFileDto
        {
            FileName = $"customers-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv",
            Content = content
        };
    }
}
