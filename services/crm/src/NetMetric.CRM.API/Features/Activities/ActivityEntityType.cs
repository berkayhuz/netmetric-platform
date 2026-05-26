// <copyright file="ActivityEntityType.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Features.Activities;

public enum ActivityEntityType
{
    Customer,
    Company,
    Contact,
    Lead,
    Deal,
    Opportunity,
    Quote,
    Ticket,
    Task
}

public static class ActivityEntityTypeParser
{
    public static bool TryParse(string? value, out ActivityEntityType entityType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            entityType = default;
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out entityType);
    }
}
