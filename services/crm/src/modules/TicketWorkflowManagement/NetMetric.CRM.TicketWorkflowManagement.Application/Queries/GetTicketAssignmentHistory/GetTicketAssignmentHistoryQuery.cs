// <copyright file="GetTicketAssignmentHistoryQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketAssignmentHistory;

public sealed record GetTicketAssignmentHistoryQuery(Guid TicketId) : IRequest<IReadOnlyList<TicketAssignmentHistoryDto>>;
