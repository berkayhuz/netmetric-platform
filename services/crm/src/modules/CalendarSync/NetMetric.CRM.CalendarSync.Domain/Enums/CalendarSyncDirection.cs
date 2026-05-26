// <copyright file="CalendarSyncDirection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CalendarSync.Domain.Enums;

public enum CalendarSyncDirection
{
    ImportOnly = 1,
    ExportOnly = 2,
    Bidirectional = 3
}
