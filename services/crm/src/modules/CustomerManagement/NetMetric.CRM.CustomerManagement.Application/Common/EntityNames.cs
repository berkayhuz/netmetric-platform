// <copyright file="EntityNames.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.Common;

public static class EntityNames
{
    public const string Company = "company";
    public const string Contact = "contact";
    public const string Customer = "customer";

    public static bool IsSupported(string entityName)
        => string.Equals(entityName, Company, StringComparison.OrdinalIgnoreCase)
           || string.Equals(entityName, Contact, StringComparison.OrdinalIgnoreCase)
           || string.Equals(entityName, Customer, StringComparison.OrdinalIgnoreCase);
}
