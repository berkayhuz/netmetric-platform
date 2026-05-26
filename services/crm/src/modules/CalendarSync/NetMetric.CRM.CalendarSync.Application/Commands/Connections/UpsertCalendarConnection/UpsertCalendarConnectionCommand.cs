// <copyright file="UpsertCalendarConnectionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CalendarSync.Contracts.DTOs;
using NetMetric.CRM.CalendarSync.Domain.Enums;

namespace NetMetric.CRM.CalendarSync.Application.Commands.Connections.UpsertCalendarConnection;

public sealed record UpsertCalendarConnectionCommand(Guid? Id, string Name, CalendarProviderType Provider, string CalendarIdentifier, string SecretReference, CalendarSyncDirection SyncDirection, bool IsActive) : IRequest<CalendarConnectionDto>;
