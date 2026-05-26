// <copyright file="TicketManagementMappingExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.TicketManagement.Domain.Common;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Services;

internal static class TicketManagementMappingExtensions
{
    public static string GenerateTicketNumber()
        => $"{TicketNumberDefaults.Prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
}
