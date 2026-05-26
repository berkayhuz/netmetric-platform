// <copyright file="CalendarSyncRunDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CalendarSync.Contracts.DTOs;

public sealed record CalendarSyncRunDto(Guid Id, Guid ConnectionId, string Status, int ImportedCount, int ExportedCount, string? ErrorMessage);
