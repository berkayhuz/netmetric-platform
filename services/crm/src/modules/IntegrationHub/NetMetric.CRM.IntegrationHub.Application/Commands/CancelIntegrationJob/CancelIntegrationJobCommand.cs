// <copyright file="CancelIntegrationJobCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.CancelIntegrationJob;

public sealed record CancelIntegrationJobCommand(Guid TenantId, Guid JobId, string? Reason) : IRequest;
