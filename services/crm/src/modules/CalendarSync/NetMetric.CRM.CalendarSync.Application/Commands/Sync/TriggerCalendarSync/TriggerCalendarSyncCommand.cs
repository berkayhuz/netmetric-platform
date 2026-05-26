// <copyright file="TriggerCalendarSyncCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CalendarSync.Contracts.DTOs;

namespace NetMetric.CRM.CalendarSync.Application.Commands.Sync.TriggerCalendarSync;

public sealed record TriggerCalendarSyncCommand(Guid ConnectionId) : IRequest<CalendarSyncRunDto>;
