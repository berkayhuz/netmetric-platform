// <copyright file="GetTicketStatusHistoryQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketStatusHistory;

public sealed record GetTicketStatusHistoryQuery(Guid TicketId) : IRequest<IReadOnlyList<TicketStatusHistoryDto>>;
