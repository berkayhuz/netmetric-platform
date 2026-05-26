// <copyright file="UpdateTicketQueueCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Enums;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.UpdateTicketQueue;

public sealed record UpdateTicketQueueCommand(
    Guid QueueId,
    string Name,
    string? Description,
    TicketQueueAssignmentStrategy AssignmentStrategy,
    bool IsDefault) : IRequest;
