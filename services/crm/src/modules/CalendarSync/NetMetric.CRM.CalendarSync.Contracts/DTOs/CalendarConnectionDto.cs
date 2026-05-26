// <copyright file="CalendarConnectionDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CalendarSync.Contracts.DTOs;

public sealed record CalendarConnectionDto(Guid Id, string Name, string Provider, string CalendarIdentifier, string SyncDirection, bool IsActive);
