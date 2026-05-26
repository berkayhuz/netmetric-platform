// <copyright file="CalendarOverviewDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CalendarSync.Contracts.DTOs;

public sealed record CalendarOverviewDto(IReadOnlyList<CalendarConnectionDto> Connections, IReadOnlyList<CalendarSyncRunDto> RecentRuns, int ActiveConnectionCount);
