// <copyright file="ScheduleIntegrationJobCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Idempotency;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.ScheduleIntegrationJob;

public sealed record ScheduleIntegrationJobCommand(
    Guid TenantId,
    string JobType,
    string Direction,
    string PayloadJson,
    DateTime ScheduledAtUtc,
    string? ProviderKey = null,
    string? IdempotencyKey = null,
    int? MaxAttempts = null) : IRequest<Guid>, IIdempotentCommand;
