// <copyright file="AssignTicketToQueueCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Assignments.AssignTicketToQueue;

public sealed record AssignTicketToQueueCommand(Guid TicketId, Guid? PreviousQueueId, Guid NewQueueId, string? Reason) : IRequest;
