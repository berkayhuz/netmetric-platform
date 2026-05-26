// <copyright file="GetSlaPoliciesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketSlaManagement.Application.Queries.SlaPolicies;

public sealed record GetSlaPoliciesQuery() : IRequest<IReadOnlyList<SlaPolicyListItemDto>>;
