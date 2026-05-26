// <copyright file="CustomerManagementSecurityService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class CustomerManagementSecurityService(
    ICurrentUserService currentUserService,
    ICustomerManagementDbContext dbContext,
    IOptions<CustomerManagementSecurityOptions> options) : ICustomerManagementSecurityService
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICustomerManagementDbContext _dbContext = dbContext;
    private readonly CustomerManagementSecurityOptions _options = options.Value;

    public IQueryable<Company> ApplyCompanyReadScope(IQueryable<Company> query)
    {
        if (CanBypassRowSecurity())
            return query;

        var userId = _currentUserService.UserId;
        return query.Where(x => x.OwnerUserId == userId || (_options.AllowUnassignedRead && x.OwnerUserId == null));
    }

    public IQueryable<Contact> ApplyContactReadScope(IQueryable<Contact> query)
    {
        if (CanBypassRowSecurity())
            return query;

        var userId = _currentUserService.UserId;
        return query.Where(x =>
            x.OwnerUserId == userId ||
            (_options.AllowUnassignedRead && x.OwnerUserId == null) ||
            (x.Company != null && x.Company.OwnerUserId == userId) ||
            (x.Customer != null && x.Customer.OwnerUserId == userId));
    }

    public IQueryable<Customer> ApplyCustomerReadScope(IQueryable<Customer> query)
    {
        if (CanBypassRowSecurity())
            return query;

        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;
        var now = DateTime.UtcNow;
        return query.Where(x =>
            x.OwnerUserId == userId ||
            (_options.AllowUnassignedRead && x.OwnerUserId == null) ||
            (x.Company != null && x.Company.OwnerUserId == userId) ||
            _dbContext.CustomerRecordShares.Any(share =>
                share.TenantId == tenantId &&
                !share.IsDeleted &&
                share.IsActive &&
                share.EntityType == CustomerEntityType.Customer &&
                share.EntityId == x.Id &&
                share.SharedWithUserId == userId &&
                (!share.ValidUntilUtc.HasValue || share.ValidUntilUtc.Value > now) &&
                (share.AccessLevel == CustomerRecordAccessLevel.Read ||
                 share.AccessLevel == CustomerRecordAccessLevel.Comment ||
                 share.AccessLevel == CustomerRecordAccessLevel.Edit ||
                 share.AccessLevel == CustomerRecordAccessLevel.OwnerDelegate)));
    }

    public bool CanReadAuditLogs()
        => _currentUserService.HasPermission(Permissions.AuditLogsRead) ||
           _currentUserService.HasPermission(Permissions.FieldSecurityBypass);

    public string? Mask(string entityName, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || CanViewSensitiveField(entityName, fieldName))
            return value;

        return fieldName switch
        {
            var name when name.Contains("Email", StringComparison.OrdinalIgnoreCase) => MaskEmail(value),
            var name when name.Contains("Phone", StringComparison.OrdinalIgnoreCase) => MaskSuffix(value, 2),
            var name when name.Contains("TaxNumber", StringComparison.OrdinalIgnoreCase) => MaskSuffix(value, 4),
            var name when name.Contains("IdentityNumber", StringComparison.OrdinalIgnoreCase) => MaskSuffix(value, 4),
            _ => "***"
        };
    }

    private bool CanBypassRowSecurity()
        => _currentUserService.HasPermission(Permissions.RowSecurityBypass);

    private bool CanViewSensitiveField(string entityName, string fieldName)
    {
        if (_currentUserService.HasPermission(Permissions.FieldSecurityBypass) ||
            _currentUserService.HasPermission(Permissions.SensitiveDataView))
        {
            return true;
        }

        return !IsSensitiveField(entityName, fieldName);
    }

    private static bool IsSensitiveField(string entityName, string fieldName)
    {
        return (entityName, fieldName) switch
        {
            (_, var field) when field.Contains("Email", StringComparison.OrdinalIgnoreCase) => true,
            (_, var field) when field.Contains("Phone", StringComparison.OrdinalIgnoreCase) => true,
            ("Company", nameof(Company.TaxNumber)) => true,
            ("Customer", nameof(Customer.IdentityNumber)) => true,
            _ => false
        };
    }

    private static string MaskEmail(string value)
    {
        var atIndex = value.IndexOf('@');
        if (atIndex <= 1)
            return "***";

        return $"{value[..1]}***{value[atIndex..]}";
    }

    private static string MaskSuffix(string value, int suffixLength)
    {
        if (value.Length <= suffixLength)
            return new string('*', value.Length);

        return new string('*', value.Length - suffixLength) + value[^suffixLength..];
    }
}
