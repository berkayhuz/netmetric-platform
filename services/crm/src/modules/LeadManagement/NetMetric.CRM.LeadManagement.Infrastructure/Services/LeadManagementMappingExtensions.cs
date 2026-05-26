// <copyright file="LeadManagementMappingExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Infrastructure.Services;

internal static class LeadManagementMappingExtensions
{
    public static string GenerateLeadCode()
        => $"LEAD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..27];

    public static string GenerateOpportunityCode()
        => $"OPP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..26];

    public static (string FirstName, string LastName) SplitName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length switch
        {
            0 => ("Unknown", "Customer"),
            1 => (parts[0], string.Empty),
            _ => (parts[0], string.Join(' ', parts.Skip(1)))
        };
    }
}
