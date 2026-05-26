// <copyright file="ExportContactsQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.CustomerManagement.Application.Features.Exports.Queries.ExportContacts;

public sealed class ExportContactsQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService)
    : IRequestHandler<ExportContactsQuery, ExportFileDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementSecurityService _securityService = securityService;

    public async Task<ExportFileDto> Handle(ExportContactsQuery request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureAuthenticated();
        var tenantId = _currentUserService.TenantId;

        var query = _securityService.ApplyContactReadScope(_dbContext.Set<Contact>())
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
                (x.WorkPhone != null && EF.Functions.Like(x.WorkPhone, search)) ||
                (x.JobTitle != null && EF.Functions.Like(x.JobTitle, search)));
        }

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

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
                x.WorkPhone,
                x.JobTitle,
                x.Department,
                x.OwnerUserId,
                x.CompanyId,
                x.CustomerId,
                x.IsPrimaryContact,
                x.IsActive,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var content = CsvExportBuilder.Build(
            headers: ["Id", "FirstName", "LastName", "Email", "MobilePhone", "WorkPhone", "JobTitle", "Department", "OwnerUserId", "CompanyId", "CustomerId", "IsPrimaryContact", "IsActive", "CreatedAtUtc"],
            rows: rows,
            selector: x =>
            [
                x.Id.ToString(),
                x.FirstName,
                x.LastName,
                _securityService.Mask(nameof(Contact), nameof(Contact.Email), x.Email),
                _securityService.Mask(nameof(Contact), nameof(Contact.MobilePhone), x.MobilePhone),
                _securityService.Mask(nameof(Contact), nameof(Contact.WorkPhone), x.WorkPhone),
                x.JobTitle,
                x.Department,
                x.OwnerUserId?.ToString(),
                x.CompanyId?.ToString(),
                x.CustomerId?.ToString(),
                x.IsPrimaryContact.ToString(),
                x.IsActive.ToString(),
                x.CreatedAt.ToString("O")
            ]);

        return new ExportFileDto
        {
            FileName = $"contacts-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv",
            Content = content
        };
    }
}
