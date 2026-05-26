// <copyright file="ICustomerManagementSecurityService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;

public interface ICustomerManagementSecurityService
{
    IQueryable<Company> ApplyCompanyReadScope(IQueryable<Company> query);
    IQueryable<Contact> ApplyContactReadScope(IQueryable<Contact> query);
    IQueryable<Customer> ApplyCustomerReadScope(IQueryable<Customer> query);
    bool CanReadAuditLogs();
    string? Mask(string entityName, string fieldName, string? value);
}
