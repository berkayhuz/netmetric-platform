// <copyright file="GetTicketSlaWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketSlaManagement.Application.Queries.TicketSla;

public sealed record GetTicketSlaWorkspaceQuery(Guid TicketId) : IRequest<TicketSlaWorkspaceDto?>;
