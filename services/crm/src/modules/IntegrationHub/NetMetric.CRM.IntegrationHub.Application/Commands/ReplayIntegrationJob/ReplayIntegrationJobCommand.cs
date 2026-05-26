// <copyright file="ReplayIntegrationJobCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Idempotency;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.ReplayIntegrationJob;

public sealed record ReplayIntegrationJobCommand(Guid TenantId, Guid JobId, string? IdempotencyKey = null) : IRequest<Guid>, IIdempotentCommand;
