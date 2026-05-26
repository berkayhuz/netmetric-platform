// <copyright file="GetConnectorHealthQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.IntegrationHub.Application.DTOs;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.GetConnectorHealth;

public sealed record GetConnectorHealthQuery(Guid TenantId) : IRequest<IReadOnlyCollection<IntegrationConnectorHealthDto>>;
