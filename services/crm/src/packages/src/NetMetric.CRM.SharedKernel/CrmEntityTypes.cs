// <copyright file="CrmEntityTypes.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SharedKernel;

public static class CrmEntityTypes
{
    public const string Customer = "customer";
    public const string Company = "company";
    public const string Contact = "contact";
    public const string Opportunity = "opportunity";
    public const string Ticket = "ticket";
    public const string Document = "document";

    public static bool IsSupported(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return false;

        return entityType.Trim().ToLowerInvariant() switch
        {
            Customer => true,
            Company => true,
            Contact => true,
            Opportunity => true,
            Ticket => true,
            Document => true,
            _ => false
        };
    }
}
