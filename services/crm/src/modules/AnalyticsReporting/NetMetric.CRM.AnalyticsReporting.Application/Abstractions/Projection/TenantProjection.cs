// <copyright file="TenantProjection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Projection;

public sealed record TenantProjection(
    Guid TenantId,
    string TenantName,
    int ActiveUsers,
    int Customers,
    decimal Revenue,
    int OpenTickets);
