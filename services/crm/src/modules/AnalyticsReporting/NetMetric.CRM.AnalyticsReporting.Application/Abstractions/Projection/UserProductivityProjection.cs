// <copyright file="UserProductivityProjection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Projection;

public sealed record UserProductivityProjection(
    Guid TenantId,
    Guid UserId,
    string UserName,
    int ActivitiesCompleted,
    int TicketsClosed,
    int DealsWon);
