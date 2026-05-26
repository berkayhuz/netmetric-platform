// <copyright file="GetCalendarOverviewQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CalendarSync.Contracts.DTOs;

namespace NetMetric.CRM.CalendarSync.Application.Queries.GetCalendarOverview;

public sealed record GetCalendarOverviewQuery : IRequest<CalendarOverviewDto>;
