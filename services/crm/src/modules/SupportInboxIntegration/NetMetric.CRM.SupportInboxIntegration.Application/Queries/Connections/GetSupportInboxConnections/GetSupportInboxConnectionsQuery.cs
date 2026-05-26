// <copyright file="GetSupportInboxConnectionsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Queries.Connections.GetSupportInboxConnections;

public sealed record GetSupportInboxConnectionsQuery() : IRequest<IReadOnlyList<SupportInboxConnectionDto>>;
